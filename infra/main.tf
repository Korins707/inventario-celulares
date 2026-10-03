# ---------------------------------------------------------------------------
# Infraestructura de la aplicacion de inventario de equipos celulares.
# Aprovisionada en Azure con Terraform.
# ---------------------------------------------------------------------------

provider "azurerm" {
  features {}
}

# --- Grupo de recursos -------------------------------------------------------
resource "azurerm_resource_group" "principal" {
  name     = "rg-${var.project_name}-${var.environment}"
  location = var.location
  tags = {
    proyecto   = var.project_name
    entorno    = var.environment
    gestionado = "terraform"
  }
}

# --- Registro de contenedores ------------------------------------------------
resource "azurerm_container_registry" "principal" {
  name                = "cr${replace(var.project_name, "-", "")}${var.environment}"
  resource_group_name = azurerm_resource_group.principal.name
  location            = azurerm_resource_group.principal.location
  sku                 = "Basic"
  admin_enabled       = true
  tags                = azurerm_resource_group.principal.tags
}

# --- Almacenamiento de los secretos de la base de datos -----------------------
resource "azurerm_storage_account" "secretos" {
  name                     = "st${replace(var.project_name, "-", "")}${var.environment}01"
  resource_group_name      = azurerm_resource_group.principal.name
  location                 = azurerm_resource_group.principal.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  account_kind             = "StorageV2"
  min_tls_version          = var.min_tls_version

  blob_properties {
    versioning_enabled = true
  }

  tags = azurerm_resource_group.principal.tags
}

resource "azurerm_storage_account_network_rules" "secretos" {
  storage_account_id = azurerm_storage_account.secretos.id
  default_action     = "Deny"
  bypass             = ["AzureServices"]
}

# --- Identidad administrada para la aplicacion --------------------------------
resource "azurerm_user_assigned_identity" "api" {
  name                = "id-${var.project_name}-${var.environment}"
  resource_group_name = azurerm_resource_group.principal.name
  location            = azurerm_resource_group.principal.location
  tags                = azurerm_resource_group.principal.tags
}

# --- Servidor PostgreSQL Flexible Server -------------------------------------
resource "azurerm_postgresql_flexible_server" "bd" {
  name                          = "pg-${var.project_name}-${var.environment}"
  resource_group_name           = azurerm_resource_group.principal.name
  location                      = azurerm_resource_group.principal.location
  version                       = "16"
  administrator_login           = var.admin_username
  administrator_password        = random_password.bd.result
  zone                          = "1"
  storage_mb                    = 32768
  sku_name                      = var.sku_name
  storage_tier                  = "P10"
  backup_retention_days         = 7
  geo_redundant_backup_enabled  = false
  auto_grow_enabled             = true
  public_network_access_enabled = true

  tags = azurerm_resource_group.principal.tags
}

# La red publica solo acepta el rango indicado, nunca 0.0.0.0/0.
resource "azurerm_postgresql_flexible_server_firewall_rule" "permitido" {
  name             = "permitir-${var.environment}"
  server_id        = azurerm_postgresql_flexible_server.bd.id
  start_ip_address = cidrhost(var.allowed_cidr, 0)
  end_ip_address   = cidrhost(var.allowed_cidr, 255)
}

# --- Base de datos y usuario de la aplicacion --------------------------------
resource "azurerm_postgresql_flexible_server_database" "app" {
  name      = var.db_name
  server_id = azurerm_postgresql_flexible_server.bd.id
  collation = "C"
}

resource "azurerm_postgresql_flexible_server_configuration" "ssl" {
  name      = "sslmode"
  server_id = azurerm_postgresql_flexible_server.bd.id
  value     = "require"
}

resource "random_password" "bd" {
  length  = 24
  special = true
}

resource "random_password" "app" {
  length  = 24
  special = true
}

# --- Instancia de contenedor para la API .NET ---------------------------------
resource "azurerm_container_group" "api" {
  name                = "cg-${var.project_name}-${var.environment}"
  resource_group_name = azurerm_resource_group.principal.name
  location            = azurerm_resource_group.principal.location
  os_type             = "Linux"
  sku                 = "Standard"
  restart_policy      = "OnFailure"
  ip_address_type     = "Public"
  dns_name_label      = "api-${replace(var.project_name, "-", "")}-${var.environment}"

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.api.id]
  }

  container {
    name   = "api"
    image  = "${azurerm_container_registry.principal.name}.azurecr.io/inventarioapi:latest"
    cpu    = 1
    memory = "1.5"

    environment_variables = {
      "ASPNETCORE_ENVIRONMENT"               = "Production"
      "ASPNETCORE_URL"                       = "http://+:8080"
      "ConnectionStrings__DefaultConnection" = "Host=${azurerm_postgresql_flexible_server.bd.name}.postgres.database.azure.com;Port=5432;Database=${var.db_name};Username=${azurerm_postgresql_flexible_server.bd.administrator_login};Password=${random_password.bd.result};SSL Mode=require"
    }

    ports {
      port     = 8080
      protocol = "Tcp"
    }
  }

  tags = azurerm_resource_group.principal.tags
}

# --- Grupo de seguridad de la API (solo HTTPS publicado) ----------------------
resource "azurerm_network_security_group" "api" {
  name                = "nsg-${var.project_name}-${var.environment}"
  location            = azurerm_resource_group.principal.location
  resource_group_name = azurerm_resource_group.principal.name
  tags                = azurerm_resource_group.principal.tags
}

resource "azurerm_network_security_rule" "denegado" {
  name                        = "denegar-todo-el-resto"
  priority                    = 4096
  direction                   = "Inbound"
  access                      = "Deny"
  protocol                    = "*"
  source_port_range           = "*"
  destination_port_range      = "*"
  source_address_prefix       = "0.0.0.0/0"
  destination_address_prefix  = "*"
  resource_group_name         = azurerm_resource_group.principal.name
  network_security_group_name = azurerm_network_security_group.api.name
}

# --- Registro de diagnostico (Application Insights) ---------------------------
resource "azurerm_log_analytics_workspace" "principal" {
  name                = "log-${replace(var.project_name, "-", "")}-${var.environment}"
  location            = azurerm_resource_group.principal.location
  resource_group_name = azurerm_resource_group.principal.name
  sku                 = "PerGB2018"
  retention_in_days   = 30
  tags                = azurerm_resource_group.principal.tags
}

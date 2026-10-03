output "resource_group_name" {
  description = "Nombre del grupo de recursos principal."
  value       = azurerm_resource_group.principal.name
}

output "container_registry_login_server" {
  description = "Servidor de acceso al registro de contenedores."
  value       = azurerm_container_registry.principal.login_server
}

output "container_registry_name" {
  description = "Nombre del registro de contenedores."
  value       = azurerm_container_registry.principal.name
}

output "postgresql_server_fqdn" {
  description = "Nombre de dominio completo del servidor PostgreSQL."
  value       = azurerm_postgresql_flexible_server.bd.fqdn
}

output "api_url" {
  description = "URL publica de la API desplegada en Azure Container Instances."
  value       = "https://${azurerm_container_group.api.ip_address}:8080"
}

output "api_ip_address" {
  description = "Direccion IP publica del contenedor de la API."
  value       = azurerm_container_group.api.ip_address
}

output "connection_string" {
  description = "Cadena de conexion a PostgreSQL (debe guardarse como secret en GitHub Actions)."
  value       = "Host=${azurerm_postgresql_flexible_server.bd.fqdn};Port=5432;Database=${azurerm_postgresql_flexible_server_database.app.name};Username=${azurerm_postgresql_flexible_server.bd.administrator_login};Password=${random_password.bd.result};SSL Mode=require"
  sensitive   = true
}

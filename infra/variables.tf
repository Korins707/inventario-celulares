variable "project_name" {
  description = "Prefijo de nombres para todos los recursos."
  type        = string
  default     = "inventario-celulares"

  validation {
    condition     = can(regex("^[a-z0-9-]{3,20}$", var.project_name))
    error_message = "project_name solo admite minusculas, digitos y guiones (3 a 20 caracteres)."
  }
}

variable "location" {
  description = "Region de Azure donde se crean los recursos."
  type        = string
  default     = "eastus"
}

variable "environment" {
  description = "Ambiente del despliegue."
  type        = string
  default     = "prod"

  validation {
    condition     = contains(["dev", "staging", "prod"], var.environment)
    error_message = "environment debe ser dev, staging o prod."
  }
}

variable "admin_username" {
  description = "Usuario administrador del servidor PostgreSQL."
  type        = string
  default     = "psqladmin"
}

variable "db_name" {
  description = "Nombre de la base de datos."
  type        = string
  default     = "inventario_celulares"
}

variable "sku_name" {
  description = "Plan de capacidad del servidor de base de datos."
  type        = string
  default     = "Basic_B1ms"
}

variable "min_tls_version" {
  description = "Version minima de TLS para los servicios de datos."
  type        = string
  default     = "TLS1_2"

  validation {
    condition     = contains(["TLS1_2", "TLS1_3"], var.min_tls_version)
    error_message = "min_tls_version debe ser TLS1_2 o TLS1_3."
  }
}

variable "allowed_cidr" {
  description = "Rango IP permitido para acceder al puerto PostgreSQL."
  type        = string
  default     = "203.0.113.0/24"
}

variable "aws_region" {
  description = "AWS region for all resources."
  type        = string
  default     = "eu-west-2"
}

variable "aws_profile" {
  description = "Local AWS CLI profile used to deploy."
  type        = string
  default     = "dermalog"
}

variable "environment" {
  description = "Deployment environment (dev, staging, prod)."
  type        = string
  default     = "dev"
}

variable "photo_bucket_name" {
  description = "S3 bucket name for user photos. Must be globally unique."
  type        = string
}

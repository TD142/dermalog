output "photo_bucket_name" {
  description = "Name of the S3 bucket storing user photos."
  value       = aws_s3_bucket.photos.id
}

output "photo_bucket_region" {
  description = "AWS region the photo bucket lives in."
  value       = aws_s3_bucket.photos.region
}

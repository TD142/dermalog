data "aws_iam_user" "tom_dev" {
  user_name = "tom-dev"
}

resource "aws_iam_policy" "bedrock_invoke_anthropic" {
  name        = "dermalog-bedrock-invoke-anthropic-${var.environment}"
  description = "Allow invoking Anthropic Claude foundation models in Bedrock for Dermalog."

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "bedrock:InvokeModel",
          "bedrock:InvokeModelWithResponseStream",
        ]
        Resource = "arn:aws:bedrock:${var.aws_region}::foundation-model/anthropic.*"
      }
    ]
  })
}

resource "aws_iam_user_policy_attachment" "tom_dev_bedrock" {
  user       = data.aws_iam_user.tom_dev.user_name
  policy_arn = aws_iam_policy.bedrock_invoke_anthropic.arn
}

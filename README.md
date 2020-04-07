# SQSCopy
This is a very simple utility to transfer the contents of an SQS queue to another. Usage is:

```SQSCopy [source URL] [target URL] [AWS region] {IAM access key} {IAM secret key}```

If access and secret keys are not specified, the AWS SDK will try to access the queues based on system-wide configured credentials or machine role.

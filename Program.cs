using System;
using System.Net;
using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;

namespace SQSCopy
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null || (args.Length != 3 && args.Length != 5))
            {
                Console.WriteLine("Usage: SQSCopy [source arn] [target arn] [region name] {AWS access key} {AWS secret key}");
                Console.WriteLine("If access & secret key are omitted, AWS system-wide or role will be used if present");
                return;
            }

            var client = args.Length == 5 
                ? new AmazonSQSClient(
                    new BasicAWSCredentials(args[3], args[4]),
                    RegionEndpoint.GetBySystemName(args[2])) 
                : new AmazonSQSClient(RegionEndpoint.GetBySystemName(args[2]));

            var sourceUrl = args[0];
            var targetUrl = args[1];

            var count = 0;
            while (true)
            {
                Console.WriteLine("Reading message from source...");
                var readRequest = client.ReceiveMessageAsync(new ReceiveMessageRequest
                        {QueueUrl = sourceUrl, WaitTimeSeconds = 5, VisibilityTimeout = 20, MaxNumberOfMessages = 1})
                    .Result;

                if (readRequest.Messages.Count == 0)
                {
                    Console.WriteLine("No more messages in source.");
                    break;
                }

                Console.WriteLine("Writing message to target...");
                var writeRequest = client.SendMessageAsync(new SendMessageRequest
                    {QueueUrl = targetUrl, MessageBody = readRequest.Messages[0].Body}).Result;

                if (writeRequest.HttpStatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine("Deleting message from source...");
                    Console.WriteLine("===========================");
                    Console.WriteLine(readRequest.Messages[0].Body);
                    Console.WriteLine("===========================");

                    _ = client.DeleteMessageAsync(new DeleteMessageRequest
                        {QueueUrl = sourceUrl, ReceiptHandle = readRequest.Messages[0].ReceiptHandle}).Result;

                    count++;
                }
            }

            Console.WriteLine($"{count} messages copied.");
        }
    }
}

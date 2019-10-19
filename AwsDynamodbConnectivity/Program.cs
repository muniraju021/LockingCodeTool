using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.DynamoDBv2;
using Amazon.Runtime;

// Add using statements to access AWS SDK for .NET services. 
// Both the Service and its Model namespace need to be added 
// in order to gain access to a service. For example, to access
// the EC2 service, add:
// using Amazon.EC2;
// using Amazon.EC2.Model;

namespace AwsDynamodbConnectivity
{
    class Program
    {
        public static void Main(string[] args)
        {
            //IAmazonS3 s3Client = new AmazonS3Client();
            //Console.Read();

            try
            {
                var credentials = new BasicAWSCredentials("auxinnotest@gmail.com", "Auxinnotest@19");
                var client = new AmazonDynamoDBClient(credentials, RegionEndpoint.USWest2);
                var tableResponse = client.ListTables();

            }
            catch (Exception ex)
            {
                Console.WriteLine("     FAILED to create a DynamoDB client; " + ex.Message);
                //operationFailed = true;
            }
        }
    }
}
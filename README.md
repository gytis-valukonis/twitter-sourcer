# twitter-sourcer
The goal of this project is to source tweets straight from twitter API straight to AWS DynamoDB.

## Overview

The main entrypoint for this application is the ```TwitterSourcer.RetrievalWorker``` project which is mainly responsible for getting the events from twitter and putting them into the SQS queue. The project, for constant running is hosted on EC2 instance through system.d configuration. So, on startup when the instance is up, it will bring up the service.

After the message arrives in the SQS queue it goes to the ```TwitterSourcer.Transform``` Lambda function which is responsible for processing the event message ant putting it into the DynamoDB query and additionally adding some metrics into Cloudwatch for it to be used in graphs.

![image](https://user-images.githubusercontent.com/10450448/206407960-389413c9-ad5b-4ed5-994b-ee23dee4f220.png)

Additionally, there is a serverless .NET API hosted on Lambda to allow accessing tweets that are collected for each date (required query string parameter date) and change twitter filter rules. More logic to it - if there aren't any filter rules, the ec2 instance is configured to be stopped. If there are new rules (when there weren't any), ec2 instance will be started.

The lambda then, is exposed through API Gateway and requires and API key to change rules.

## To improve

There are a lot of To do's and comments throughout the project to improve code quality.

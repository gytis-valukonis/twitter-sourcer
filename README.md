# twitter-sourcer
The goal of this project is to source tweets straight from twitter API straight to AWS DynamoDB.

## Overview

The main entrypoint for this application is the ```TwitterSourcer.RetrievalWorker``` project which is mainly responsible for getting the events from twitter and putting them into the SQS queue. The project, for constant running is hosted on EC2 instance through system.d configuration. So, on startup when the instance is up, it will bring up the service.

After the message arrives in the SQS queue it goes to the ```TwitterSourcer.Transform``` Lambda function which is responsible for processing the event message ant putting it into the DynamoDB query and additionally adding some metrics into Cloudwatch for it to be used in graphs.

![image](https://user-images.githubusercontent.com/10450448/206407960-389413c9-ad5b-4ed5-994b-ee23dee4f220.png)

Additionally, there is a serverless .NET API hosted on Lambda to allow accessing tweets that are collected for each date (required query string parameter date) and change twitter filter rules. More logic to it - if there aren't any filter rules, the ec2 instance is configured to be stopped. If there are new rules (when there weren't any), ec2 instance will be started.

The lambda then, is exposed through API Gateway and requires and API key to change rules.

## API Endpoints

The API can be accessed here: https://k7e61diwe0.execute-api.eu-central-1.amazonaws.com/default

### _GET_ /tweets
This endpoint retrieves tweets by day. Requires query parameter _date_ to work.
Example: ```GET /tweets?date=2022-12-08```

### _GET_ /applied-filter-rules
This endpoint gets currently set filter rules for the tweet streaming.
Example: ```GET /applied-filter-rules```

### _POST_ /applied-filter-rules
This endpoint requires API Key to work. Add an ```x-api-key``` header to the request.
Example: ```POST /applied-filter-rules```
Body: ```{ value: "example" }```
Afterwards, if there is no filter set previously - starts the retrieval process

### _DELETE_ /applied-filter-rules/{id}
This endpoint requires API Key to work. Add an ```x-api-key``` header to the request.
Example: ```DELETE /applied-filter-rules/123456789123```
Afterwards, if there are no filters anymore - stops the retrieval process

## To improve

There are a lot of To do's and comments throughout the project to improve code quality.
Would document API Gateway more to have better openapi specification. Currently code generated swagger works locally with more information on request/response types.

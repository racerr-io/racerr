import { CloudFrontToS3 } from "@aws-solutions-constructs/aws-cloudfront-s3";
import { App, RemovalPolicy, Stack } from "aws-cdk-lib";
import { BucketDeployment, Source } from "aws-cdk-lib/aws-s3-deployment";
import { assert } from "console";

export const createClientStack = (app: App) => {
  const clientStack = new Stack(app, "racerr-client-stack", {
    env: {
      account: "654654225422",
      region: "ap-southeast-2",
    },
  });

  const { cloudFrontWebDistribution, s3Bucket } = new CloudFrontToS3(clientStack, "racerr-client", {
    bucketProps: {
      removalPolicy: RemovalPolicy.DESTROY,
    },
    logS3AccessLogs: false,
    cloudFrontDistributionProps: {
      enableLogging: false
    },
    insertHttpSecurityHeaders: false
  });

  assert(s3Bucket);

  new BucketDeployment(clientStack, 'racerr-client-deployment', {
    sources: [Source.asset('../build/WebGL/WebGL')],
    destinationBucket: s3Bucket!,
    distribution: cloudFrontWebDistribution
  })
};

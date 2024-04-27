import { CloudFrontToS3 } from "@aws-solutions-constructs/aws-cloudfront-s3";
import { App, RemovalPolicy, Stack } from "aws-cdk-lib";
import { BucketDeployment, Source } from "aws-cdk-lib/aws-s3-deployment";
import { assert } from "console";

export const createClientStack = (app: App) => {
  const lootStack = new Stack(app, "LootStack", {
    env: {
      account: "654654225422",
      region: "ap-southeast-2",
    },
    crossRegionReferences: true,
  });

  const { cloudFrontWebDistribution, s3Bucket } = new CloudFrontToS3(lootStack, "loot-cloudfront-s3", {
    bucketProps: {
      removalPolicy: RemovalPolicy.DESTROY,
    },
    insertHttpSecurityHeaders: false
  });

  assert(s3Bucket);

  // new BucketDeployment(lootStack, 'LootStackS3BucketDeployment', {
  //   sources: [Source.asset('../build')],
  //   destinationBucket: s3Bucket!,
  //   distribution: cloudFrontWebDistribution
  // })
};

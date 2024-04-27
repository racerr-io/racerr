import { App, Stack } from "aws-cdk-lib";
import { Port } from "aws-cdk-lib/aws-ec2";
import { ContainerImage } from "aws-cdk-lib/aws-ecs";
import { ApplicationLoadBalancedFargateService } from "aws-cdk-lib/aws-ecs-patterns";

const HTTP_SERVER_PORT = 3000;
const GAME_SERVER_PORT = 7778;

export const createServerStack = (app: App) => {
  const serverStack = new Stack(app, "racerr-server-stack", {
    env: {
      account: "654654225422",
      region: "ap-southeast-2",
    },
  });

  if (!serverStack.bundlingRequired) {
    return;
  }

  const buildImage = process.env.BUILD_IMAGE;
  if (!buildImage) {
    throw Error("BUILD_IMAGE must be provided.");
  }

  const { loadBalancer, service, targetGroup } = new ApplicationLoadBalancedFargateService(serverStack, 'racerr-server', {
    memoryLimitMiB: 512,
    desiredCount: 1,
    cpu: 256,
    taskImageOptions: {
      image: ContainerImage.fromRegistry(buildImage),
      containerPort: GAME_SERVER_PORT
    },
    listenerPort: GAME_SERVER_PORT
  });

  service.taskDefinition.defaultContainer!.addPortMappings({
    containerPort: HTTP_SERVER_PORT
  });
  targetGroup.configureHealthCheck({
    path: "/healthcheck",
    port: HTTP_SERVER_PORT.toString()
  });
  loadBalancer.connections.allowTo(service, Port.tcp(HTTP_SERVER_PORT));
  service.connections.allowFrom(loadBalancer, Port.tcp(HTTP_SERVER_PORT));
};


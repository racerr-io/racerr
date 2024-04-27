#!/usr/bin/env node
import { App } from "aws-cdk-lib";
import "source-map-support/register";
import { createClientStack } from "../lib/client-stack";
import { createServerStack } from "../lib/server-stack";

const app = new App();
createClientStack(app);
createServerStack(app);
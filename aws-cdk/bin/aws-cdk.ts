#!/usr/bin/env node
import { App } from "aws-cdk-lib";
import "source-map-support/register";
import { createClientStack } from "../lib/client-stack";

const app = new App();
createClientStack(app);

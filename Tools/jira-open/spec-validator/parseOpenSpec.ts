import { readFile } from "node:fs/promises";
import path from "node:path";
import { load } from "js-yaml";
import { SPEC_VALIDATION_CONFIG } from "./config.ts";

export type SpecScenario = {
  id: string;
  title?: string;
  source: string;
};

export type SpecParseResult = {
  specIds: Set<string>;
  scenarios: SpecScenario[];
  bySource: Record<string, string[]>;
};

type ScenarioYaml = {
  id?: unknown;
  title?: unknown;
};

const SPEC_ID_REGEX = /^[A-Z]+-\d+$/;

const isRecord = (value: unknown): value is Record<string, unknown> => {
  return typeof value === "object" && value !== null;
};

const toScenario = (value: unknown, source: string): SpecScenario | null => {
  if (!isRecord(value)) {
    return null;
  }

  const scenario = value as ScenarioYaml;
  if (typeof scenario.id !== "string") {
    return null;
  }

  const id = scenario.id.trim();
  if (!SPEC_ID_REGEX.test(id)) {
    return null;
  }

  const title = typeof scenario.title === "string" ? scenario.title.trim() : undefined;
  return { id, title, source };
};

const extractScenarios = (rawYaml: unknown, source: string): SpecScenario[] => {
  if (!isRecord(rawYaml)) {
    return [];
  }

  const scenariosValue = rawYaml.scenarios;
  if (!Array.isArray(scenariosValue)) {
    return [];
  }

  return scenariosValue
    .map(scenario => toScenario(scenario, source))
    .filter((scenario): scenario is SpecScenario => scenario !== null);
};

export const parseOpenSpec = async (projectRoot: string): Promise<SpecParseResult> => {
  const bySource: Record<string, string[]> = {};
  const scenarios: SpecScenario[] = [];

  for (const sourcePath of SPEC_VALIDATION_CONFIG.openSpecFiles) {
    const absolutePath = path.resolve(projectRoot, sourcePath);
    const raw = await readFile(absolutePath, "utf-8");
    const parsed = load(raw) as unknown;
    const parsedScenarios = extractScenarios(parsed, sourcePath);

    bySource[sourcePath] = parsedScenarios.map(({ id }) => id);
    scenarios.push(...parsedScenarios);
  }

  const specIds = new Set<string>();
  for (const scenario of scenarios) {
    specIds.add(scenario.id);
  }

  return { specIds, scenarios, bySource };
};

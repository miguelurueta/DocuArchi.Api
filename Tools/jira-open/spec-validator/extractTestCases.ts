import { readdir, readFile } from "node:fs/promises";
import path from "node:path";
import { SPEC_VALIDATION_CONFIG } from "./config.ts";

export type TestScanResult = {
  specIds: Set<string>;
  byFile: Record<string, string[]>;
  totalTaggedTests: number;
};

const TEST_FILE_REGEX = /\.test\.tsx?$/;

const globToRegex = (globPattern: string): RegExp => {
  const normalized = globPattern.replaceAll("\\", "/");
  const markerDouble = "__DOUBLE_WILDCARD__";
  const markerSingle = "__SINGLE_WILDCARD__";

  const withMarkers = normalized
    .replaceAll("**/", `${markerDouble}/`)
    .replaceAll("**", markerDouble)
    .replaceAll("*", markerSingle);

  const escaped = withMarkers.replace(/[.+^${}()|[\]\\]/g, "\\$&");
  const withWildcards = escaped
    .replaceAll(`${markerDouble}/`, "(?:.*/)?")
    .replaceAll(markerDouble, ".*")
    .replaceAll(markerSingle, "[^/]*");

  return new RegExp(`^${withWildcards}$`);
};

const collectFiles = async (dirPath: string): Promise<string[]> => {
  const entries = await readdir(dirPath, { withFileTypes: true });
  const nested = await Promise.all(
    entries.map(async entry => {
      const fullPath = path.join(dirPath, entry.name);

      if (entry.isDirectory()) {
        return collectFiles(fullPath);
      }

      if (entry.isFile() && TEST_FILE_REGEX.test(entry.name)) {
        return [fullPath];
      }

      return [];
    })
  );

  return nested.flat();
};

const extractSpecTagsFromContent = (content: string): string[] => {
  const ids: string[] = [];

  for (const match of content.matchAll(SPEC_VALIDATION_CONFIG.specTagRegex)) {
    const id = match[1];
    if (id) {
      ids.push(id);
    }
  }

  return ids;
};

export const extractTestCases = async (projectRoot: string): Promise<TestScanResult> => {
  const allSrcFiles = await collectFiles(path.resolve(projectRoot, "src"));
  const matchers = SPEC_VALIDATION_CONFIG.testGlobs.map(globToRegex);

  const byFile: Record<string, string[]> = {};
  const specIds = new Set<string>();
  let totalTaggedTests = 0;

  const candidateFiles = allSrcFiles.filter(filePath => {
    const relativePath = path.relative(projectRoot, filePath).replaceAll("\\", "/");
    return matchers.some(matcher => matcher.test(relativePath));
  });

  for (const filePath of candidateFiles) {
    const content = await readFile(filePath, "utf-8");
    const tags = extractSpecTagsFromContent(content);

    const relativePath = path.relative(projectRoot, filePath).replaceAll("\\", "/");
    byFile[relativePath] = tags;

    tags.forEach(id => specIds.add(id));
    totalTaggedTests += tags.length;
  }

  return { specIds, byFile, totalTaggedTests };
};

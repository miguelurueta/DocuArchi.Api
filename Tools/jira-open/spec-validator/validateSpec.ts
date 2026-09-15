import process from "node:process";
import { extractTestCases } from "./extractTestCases.ts";
import { parseOpenSpec } from "./parseOpenSpec.ts";

const formatList = (ids: string[]): string => {
  if (ids.length === 0) {
    return "(none)";
  }

  return ids.map(id => `- ${id}`).join("\n");
};

const run = async (): Promise<void> => {
  const projectRoot = process.cwd();
  const specResult = await parseOpenSpec(projectRoot);
  const testResult = await extractTestCases(projectRoot);

  const specIds = Array.from(specResult.specIds).sort();
  const testIds = Array.from(testResult.specIds).sort();

  const missingSpecs = specIds.filter(id => !testResult.specIds.has(id));
  const unknownTags = testIds.filter(id => !specResult.specIds.has(id));
  const coveredSpecs = specIds.length - missingSpecs.length;

  console.log("\nOpenSpec/Test Validation Report");
  console.log("================================");
  console.log(`Total specs: ${specIds.length}`);
  console.log(`Total test tags: ${testResult.totalTaggedTests}`);
  console.log(`Specs covered: ${coveredSpecs}`);
  console.log(`Specs missing: ${missingSpecs.length}`);
  console.log(`Unknown tags in tests: ${unknownTags.length}`);

  console.log("\nSpec IDs by source:");
  for (const [sourcePath, sourceIds] of Object.entries(specResult.bySource)) {
    console.log(`- ${sourcePath}: ${sourceIds.length} scenario(s)`);
  }

  console.log("\nMissing spec coverage:");
  console.log(formatList(missingSpecs));

  console.log("\nUnknown spec tags in tests:");
  console.log(formatList(unknownTags));

  console.log("\nTags by file:");
  const byFileEntries = Object.entries(testResult.byFile).sort(([a], [b]) =>
    a.localeCompare(b)
  );
  if (byFileEntries.length === 0) {
    console.log("(none)");
  } else {
    byFileEntries.forEach(([filePath, tags]) => {
      const uniqueTags = Array.from(new Set(tags)).sort();
      console.log(`- ${filePath}: ${uniqueTags.join(", ") || "(none)"}`);
    });
  }

  if (missingSpecs.length > 0 || unknownTags.length > 0) {
    process.exitCode = 1;
    return;
  }

  process.exitCode = 0;
};

run().catch(error => {
  console.error("Spec validation failed with an unexpected error.");
  console.error(error);
  process.exit(1);
});

export type SpecValidationConfig = {
  openSpecFiles: string[];
  testGlobs: string[];
  specTagRegex: RegExp;
};

export const SPEC_VALIDATION_CONFIG: SpecValidationConfig = {
  openSpecFiles: [
    "openspec/auth.behavior.yaml",
    "openspec/auth.contract.yaml",
    "openspec/dashboard.behavior.yaml",
    "openspec/dashboard.contract.yaml",
    "openspec/otp.behavior.yaml",
    "openspec/otp.contract.yaml",
    "openspec/recovery.behavior.yaml",
    "openspec/recovery.contract.yaml",
    "openspec/reset.behavior.yaml",
    "openspec/reset.contract.yaml",
  ],
  testGlobs: [
    "src/app/auth/**/*.test.ts",
    "src/app/auth/**/*.test.tsx",
    "src/modules/dashboard/**/*.test.ts",
    "src/modules/dashboard/**/*.test.tsx",
    "src/modules/login/**/*.test.ts",
    "src/modules/login/**/*.test.tsx",
    "src/modules/OTP/**/*.test.ts",
    "src/modules/OTP/**/*.test.tsx",
    "src/modules/RecoveryPassword/**/*.test.ts",
    "src/modules/RecoveryPassword/**/*.test.tsx",
  ],
  specTagRegex: /\[SPEC:([A-Z]+-\d+)\]/g,
};

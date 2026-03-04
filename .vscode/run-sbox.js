const { spawn } = require("child_process");
const path = require("path");

const SBOX_DEV_PATH = "B:\\Games\\Steam\\steamapps\\common\\sbox\\sbox-dev.exe";
const projectDir = process.cwd();
const sbproj = path.join(projectDir, ".sbproj");

const child = spawn(SBOX_DEV_PATH, ["-project", sbproj], {
  stdio: "inherit",
  cwd: projectDir,
  shell: false,
});

child.on("exit", (code) => process.exit(code != null ? code : 0));

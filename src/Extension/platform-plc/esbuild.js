const esbuild = require("esbuild");
const fs = require("fs");
const path = require("path");

const production = process.argv.includes("--production");
const watch = process.argv.includes("--watch");

// Copy README from root
try {
  const rootReadme = path.resolve(__dirname, "../../../README.md");
  const destReadme = path.resolve(__dirname, "README.md");
  if (fs.existsSync(rootReadme)) {
    fs.copyFileSync(rootReadme, destReadme);
    console.log("Copied README.md from root to extension folder.");
  } else {
    console.warn("Warning: Root README.md not found at " + rootReadme);
  }
} catch (e) {
  console.error("Error copying README.md:", e);
}

// Copy Logo from root
try {
  const rootLogo = path.resolve(__dirname, "../../../pictures/PlatformPLC.png");
  const destLogo = path.resolve(__dirname, "PlatformPLC.png");
  if (fs.existsSync(rootLogo)) {
    fs.copyFileSync(rootLogo, destLogo);
    console.log("Copied PlatformPLC.png from pictures to extension folder.");
  } else {
    console.warn("Warning: Logo not found at " + rootLogo);
  }
} catch (e) {
  console.error("Error copying logo:", e);
}

/**
 * @type {import('esbuild').Plugin}
 */
const esbuildProblemMatcherPlugin = {
  name: "esbuild-problem-matcher",

  setup(build) {
    build.onStart(() => {
      console.log("[watch] build started");
    });
    build.onEnd((result) => {
      result.errors.forEach(({ text, location }) => {
        console.error(`✘ [ERROR] ${text}`);
        console.error(
          `    ${location.file}:${location.line}:${location.column}:`,
        );
      });
      console.log("[watch] build finished");
    });
  },
};

async function main() {
  const ctx = await esbuild.context({
    entryPoints: ["src/extension.ts"],
    bundle: true,
    format: "cjs",
    minify: production,
    sourcemap: !production,
    sourcesContent: false,
    platform: "node",
    outfile: "dist/extension.js",
    external: ["vscode", "vscode-languageclient", "vscode-languageclient/node"],
    logLevel: "silent",
    plugins: [
      /* add to the end of plugins array */
      esbuildProblemMatcherPlugin,
    ],
  });
  if (watch) {
    await ctx.watch();
  } else {
    await ctx.rebuild();
    await ctx.dispose();
  }
}

main().catch((e) => {
  console.error(e);
  process.exit(1);
});

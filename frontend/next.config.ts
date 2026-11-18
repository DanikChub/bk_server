import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  output: "standalone",
  // PDFKit loads fonts and binary font dependencies through native Node resolution.
  serverExternalPackages: ["pdfkit", "fontkit", "nodemailer"],
  outputFileTracingIncludes: {
    "/api/v1/*": ["./node_modules/pdfkit/js/data/**/*"],
  },
  reactCompiler: true,
};

export default nextConfig;

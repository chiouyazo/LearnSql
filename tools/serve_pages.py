import http.server
import os
import sys

root = os.path.abspath(sys.argv[1])
base = sys.argv[2]
port = int(sys.argv[3])

content_types = {
    ".wasm": "application/wasm",
    ".js": "text/javascript",
    ".mjs": "text/javascript",
    ".json": "application/json",
    ".css": "text/css",
    ".svg": "image/svg+xml",
    ".html": "text/html; charset=utf-8",
}


class PagesHandler(http.server.BaseHTTPRequestHandler):
    def do_GET(self):
        path = self.path.split("?", 1)[0].split("#", 1)[0]
        if not path.startswith(base):
            self.send_response(302)
            self.send_header("Location", base)
            self.end_headers()
            return
        relative = path[len(base):]
        if relative == "" or relative.endswith("/"):
            relative += "index.html"
        target = os.path.normpath(os.path.join(root, relative))
        status = 200
        if not target.startswith(root) or not os.path.isfile(target):
            target = os.path.join(root, "404.html")
            status = 404
        with open(target, "rb") as handle:
            body = handle.read()
        extension = os.path.splitext(target)[1].lower()
        self.send_response(status)
        self.send_header("Content-Type", content_types.get(extension, "application/octet-stream"))
        self.send_header("Content-Length", str(len(body)))
        self.end_headers()
        self.wfile.write(body)

    def log_message(self, format, *args):
        sys.stderr.write("%s %s\n" % (self.command, self.path))


http.server.ThreadingHTTPServer(("127.0.0.1", port), PagesHandler).serve_forever()

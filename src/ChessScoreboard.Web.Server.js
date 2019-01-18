const Http = require('http');
const FileServer = require('fs');

const HostName = 'localhost';
const PortNumber = 8000;

const server = Http.createServer((request, response) => {
  if (request.url == '' || request.url == '/') {
    FileServer.readFile('index.html', function (err, html) {
      response.writeHead(200, {
        'Content-Type': 'text/html'
      });
      response.write(html);
      response.end();
    });

  } else {
    FileServer.readFile('./' + request.url, function (error, content) {
      if (!error) {
        response.setHeader('Content-type', getContentType(request.url));
        response.end(content);
      } else {
        response.writeHead(404, "File Not Found");
        response.end();
      }
    });
  }
});

function getContentType(url) {
  var dotOffset = url.lastIndexOf('.');
  var extension = dotOffset == -1 ? '.html' : url.substr(dotOffset);

  var extensionToContentType = {
    '.html': 'text/html',
    '.ico': 'image/x-icon',
    '.jpg': 'image/jpeg',
    '.png': 'image/png',
    '.gif': 'image/gif',
    '.css': 'text/css',
    '.js': 'text/javascript'
  }

  var contentType = extensionToContentType[extension];

  return contentType ? contentType : 'text/plain';
}

server.listen(PortNumber, HostName, () => {
  console.log(`Server running at http://${HostName}:${PortNumber}/`);
});
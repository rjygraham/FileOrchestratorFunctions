# FileOrchestratorFunctions

Sample Durable Azure Function to illustrate how to delay batch processing until all required events have been received. 

## Usage

Clone debug the Function App. Use cURL, Postman, VSCode REST Client, or your favorite tool to POST to the Function endpoint. You'll need to simulate sending 3 distinct files:

````
POST http://localhost:7071/api/files/2021022601-a
POST http://localhost:7071/api/files/2021022601-b
POST http://localhost:7071/api/files/2021022601-c
````

Upon receiving all 3 events (a, b, c), you will see a log message in the Function App window:

````
Beginning to process job for 2021022601!
````

Experiment with sending events out of order. You'll need to pick a new unique identifier for each "batch".

## License

The MIT License (MIT)

Copyright © 2020 Ryan Graham

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
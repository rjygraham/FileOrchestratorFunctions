# FileOrchestratorFunctions

Sample Durable Azure Function to illustrate how to delay batch processing until all required events have been received.

## Setup

1. Create new Azure Function App in Azure.
1. Create Blob container named `input` in the Storage Account created with your Azure Function App.
1. Add the following Configuration Settings to the Azure Function App:
    - "RequiredEvents": "fileAcomplete,fileB,fileC"
    - "BlobIgnoreFiles": "fileA"
1. Deploy this Function App code to the new Azure Function App you just created in Azure.

## Usage

1. Create empty text files with the file names in the format of (feel free to create multiple sets to simulate multiple overlapping jobs):

````
YYYYMMDD##-fileA.txt
YYYYMMDD##-fileB.txt
YYYYMMDD##-fileC.txt
````

2. Get the Function URL of the `HandleHttpEventAsync`, it should be in the format of:

````
https://<functionname>.azurewebsites.net/api/files/{instanceId}/{eventName}?code=<function authorization code>
````

3. Connect to the Log Stream of the Azure Function App and then upload the `YYYYMMDD##-fileA.txt` file to the `input` blob container. You should see the following logs appear in the log stream (there will be quite a few... so you'll have to look closely):

````
Received event fileA for YYYYMMDD##.
Ignoring fileA for YYYYMMDD##.
````

4. Now upload `YYYYMMDD##-fileB.txt` file to the `input` container and observe the following logs:

````
Received event fileB for YYYYMMDD##.
Processing fileB for YYYYMMDD##.
Event fileB is the first event received for YYYYMMDD##. Starting new orchestration.
Raising fileB for YYYYMMDD##.
````

5. Next upload `YYYYMMDD##-fileC.txt` file to the `input` container and observe the following logs:

````
Received event fileC for YYYYMMDD##.
Processing fileC for YYYYMMDD##.
Raising fileC for YYYYMMDD##.
````

6. Finally, use cURL, Postman, Fiddler, VS Code REST Client to send a POST to the `HandleHttpEventAsync` Function URL you retrieved above. The URL should look like:

````
https://<functionname>.azurewebsites.net/api/files/YYYYMMDD##/fileAcomplete?code=<function authorization code>
````

After you received HTTP 200 response, observe the following logs:

````
Received event fileAcomplete for YYYYMMDD##.
Raising fileAcomplete for YYYYMMDD##.
Beginning to process job for YYYYMMDD##!
````

Experiment with sending events out of order and changing the values of the `RequiredEvents` and `BlobIgnoreFiles` configuration values of the Azure Function App.

## License

The MIT License (MIT)

Copyright © 2020 Ryan Graham

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
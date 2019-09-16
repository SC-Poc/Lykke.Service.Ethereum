Converts BIL Ethereum worker indexation state .bin file to .json and vice versa.

Usage:

```
dotnet build
cd .\bin\Debug\netcoreapp2.2\ 
dotnet IndexationStateConverter.dll [Command] "<EthereumJobAzureStorageConnectionString>"
```

Available commands:

- ```read``` - reads the ".bin" file from BLOB, converts it to json and saves it to the "state.json" file`
- ```write``` - reads the "state.json" file, converts it to the binary format and saves it to the ".bin" file in BLOB.
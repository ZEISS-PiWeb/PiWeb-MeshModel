# How to run an ORT scan

## Locally

Open a terminal/cmd from the root of this repository and use the following command. `-v` maps the host directories to container paths.

```cmd
docker run -v $PWD/:/project -v $PWD/.ort:/home/ort/.ort --rm ghcr.io/oss-review-toolkit/ort --info analyze -f JSON -i /project/src -o /project/ORT
```

After analyzing the project create the report:

```cmd
docker run -v $PWD/:/project -v $PWD/.ort:/home/ort/.ort --rm ghcr.io/oss-review-toolkit/ort --info report -i /project/ORT/analyzer-result.json -f StaticHtml,PlainTextTemplate,SpdxDocument -o /project/ORT-Report
```

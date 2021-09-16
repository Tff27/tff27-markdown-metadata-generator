TFF27 Markdown Metadata Generator
============

Create a metadata JSON file from markdown files front matter.

## Main Goals
* Use with a GIT based CMS to retrieve information from markdown files to use in a static website

## Arguments
| Short Name | Name | Description |
|---|---|---|
| -i | --InputPath |  Required. Path containing markdown files (*.md). |
| -o | --OutputPath | Required. Path where JSON (*.json) metadata will be stored. |
| -n | --FileName | Required. Name for metadata file. |
| -f | --SortField | Sort metadata by a front matter header field. |
| -s | --SortOrder | Order Ascending/Descending. Default: Ascending |

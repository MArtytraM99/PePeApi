# PePe API

Simple API for getting menus from our favorite canteen PE-PE. 

## Fuctionality
 * Scraping website (jidelnapepe.cz) for menu
 * Saving menus to lower needed website scrapings to minimum
 * Exposing simple API for getting current menu and searching in past

## Setup
There are some step which you need to follow in order to be able to run PePe API locally.

### MongoDB
This application uses MongoDB in it's pesistence layer. You can run it locally or remotely (Mongo Atlas).
 * Update `ConnectionStrings` section in `appsettings.json` in `PePe.API` project according to your setup.

### Docker
If you want to run PePe API as a docker image the best way to do it is following these steps:

 1. Navigate yourself to folder containing `PePe.sln` and run following command

`docker build -t pepeapi-image -f PePe.API/Dockerfile .`

 2. Run image

`docker run -it --rm -e UseHttps="false" -e ConnectionStrings:mongoDB="mongodb://host.docker.internal:27017/" pepeapi-image`

## TODO
This is a list of features that I plan to implement in the future
 * CLI for easy usage from commandline and by other programs
 * Schedule saving menu each day (i.e. at 9:00) for later use - might be done using cron and CLI

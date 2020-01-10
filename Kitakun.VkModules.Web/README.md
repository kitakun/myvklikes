# myvklikes project
Vk widget helper app
![App Main Frame](/Readme/App1.png)


## Features:
* Set static widgets
    * Custom moderator text
    * Hilight user by moderator
    * Show top 3 likers in group
* Get top 100 likers in group for current month
* Make subscriptions (means not all can use it)

## Used dll's:
* Aufofac
* EF
* Postgres
* VkNet 1.44.0

[Tested on ubuntu 18.04](https://myvklikes.ru)

## What can be done better
* Admin panel is raw, noone can use it instead programmer
* Admin panel losing secret query on any action
* ssl & db variables can be moved to Environments

## How to migrate
cd ....\myvklikes\Kitakun.VkModules.Persistance
dotnet ef migrations add '#MigrationName#'
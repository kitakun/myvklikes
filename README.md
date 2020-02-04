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
* Implemented TOP3 & TOP5 widget
* Community admins now have separated tab with settings specialy for theirs communities
    - Including auto-update, AppToken, prices for every like/post/repost
* Calc algorythm using likes/comments/reposts (top 100 comments and reposts only)
* Highlight current user if he not top 3
* Vk query protection validation

## Used dll's:
* Aufofac
* EF
* Postgres
* VkNet 1.44.0

[Tested on ubuntu 18.04](https://myvklikes.ru)

## What can be done better
* Admin panel is raw, noone can use it instead programmer
* Admin panel losing secret query on any action
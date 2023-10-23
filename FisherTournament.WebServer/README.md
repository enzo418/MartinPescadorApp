# Description
Web server for the FisherTournament admin page

# Stack
- .Net Core 7
- Blazor Server
- FluentUI-Blazor
- Tailwind: Provides an efficient way to style the pages. 

# Development

## Tailorwind setup
Since relies in node to build the css, it is necessary to install node and npm in the machine, and then run the tailwind build command to generate the css file.

All you need to do is run: `npm install` in this folder. done.

Then just run and watch the css
- terminal 1: `$ npm run watch-and-build-css`
- terminal 2: `$ dotnet watch run`

It will work just fine since tailwind will update wwwroot/css/styles.css, which is watched by dotnet watch.

It **was added** in the project using the following steps:
1. `npm init -y`
2. `npm install -D tailwindcss`
3. + target "build-css": "npx tailwindcss -i ./wwwroot/css/site.css -o ./wwwroot/css/styles.css --minify"
4. `npx tailwindcss init`

## Fonts
1. "_Host.cshtml" sets the default body font in `body class="font-body"`, which is defined in `tailwind.config.js`
2. FluentUI use their own css variables, so to modify it you need to set the design tokens, done in `MainLayout.razor.cs`.
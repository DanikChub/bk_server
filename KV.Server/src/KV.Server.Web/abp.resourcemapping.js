module.exports = {
    aliases: {
        "@node_modules": "./node_modules",
        "@libs": "./wwwroot/libs"
    },
    mappings: {
        "@node_modules/humanize-duration/humanize-duration.js": "@libs/humanize-duration/",
        "@node_modules/@appricot/custom-stories-component/**/*": "@libs/@appricot/custom-stories-component/",
        "@node_modules/zuck.js/**/*": "@libs/zuck.js/",
        "@node_modules/keen-slider/**/*": "@libs/keen-slider/",
        "@node_modules/summernote/**/*": "@libs/summernote/",
        "@node_modules/datatables.net-buttons/js/**/*": "@libs/datatables.net-buttons/",
    }
};

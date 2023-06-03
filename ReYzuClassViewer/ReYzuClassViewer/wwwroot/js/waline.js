import { init } from 'https://unpkg.com/@waline/client@v2/dist/waline.mjs';



let _waline = {};
let _path = "/";
let _theme = "auto";


export function load()
{

    _waline = init({

        el: '#waline',
        serverURL: 'https://re-yzu-waline.vercel.app',
        path: _path,

        dark: _theme,
        emoji: false,
        search: false,
        imageUploader: false,
        meta: ['nick'],
        lang: 'zh-TW',
        locale: {
            placeholder: '我認為這個課程...',
            reactionTitle: "你認為這個課程怎麼樣？",
        },

        reaction: [
            './img/reaction/1_1f616.svg',
            './img/reaction/2_1f641.svg',
            './img/reaction/3_1f610.svg',
            './img/reaction/4_1f600.svg',
            './img/reaction/5_1f970.svg',
        ],

    });

    console.log("JavaScript - waline loaded");

}

export function updateWalineTheme( theme )
{
    let updateTheme = function ( theme )
    {
        _theme = theme;

        _waline.update({

            path: _path,
            dark: _theme,
        });
    }


    switch (theme)
    {
        case 'auto':

            updateTheme( "auto" );
            console.log('JavaScript - update waline theme , auto');

            break;

        case 'light':

            updateTheme( false );
            console.log('JavaScript - update waline theme , light');

            break;

        case 'dark':

            updateTheme( true );
            console.log('JavaScript - update waline theme , dark');

            break;

        default:
            console.log(`JavaScript - update waline theme error , arg:${theme}.`);
    }

}

export function updateWalinePath(path)
{

    _path = path;

    _waline.update({
        path: _path,
    });

    console.log(`JavaScript - update waline path , ${path}`);

}

export function destroyWaline()
{
    _waline.destroy();

    console.log('JavaScript - waline destroyed');
}



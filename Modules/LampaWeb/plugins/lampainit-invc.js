// //////////////
// Переименуйте файл lampainit-invc.js в lampainit-invc.my.js
// //////////////


var lampainit_invc = {};


// Лампа готова для использования 
lampainit_invc.appload = function appload() {
  // Lampa.Utils.putScriptAsync(["{localhost}/myplugin.js"]);  // wwwroot/myplugin.js
  // Lampa.Utils.putScriptAsync(["{localhost}/plugins/ts-preload.js", "https://nb557.github.io/plugins/online_mod.js"]);
  // Lampa.Storage.set('proxy_tmdb', 'true');
  // etc
};


// Лампа полностью загружена, можно работать с интерфейсом 
lampainit_invc.appready = function appready() {
  // $('.head .notice--icon').remove();
  addLampacDoramaMenuButton();
};


// Выполняется один раз, когда пользователь впервые открывает лампу
lampainit_invc.first_initiale = function firstinitiale() {
  // Здесь можно указать/изменить первоначальные настройки 
  // Lampa.Storage.set('source', 'tmdb');
};


// Ниже код выполняется до загрузки лампы, например можно изменить настройки 
// window.lampa_settings.push_state = false;
// localStorage.setItem('cub_domain', 'mirror-kurwa.men');
// localStorage.setItem('cub_mirrors', '["mirror-kurwa.men"]');

function openLampacDoramaMenu() {
  Lampa.Activity.push({
    url: 'discover/tv?with_original_language=ko&with_genres=18&sort_by=popularity.desc',
    title: 'Дорамы - CUB',
    component: 'category_full',
    source: 'tmdb',
    card_type: true,
    page: 1
  });
}

function addLampacDoramaMenuButton(attempt) {
  if (window.lampac_dorama_menu_ready) return;

  var menu = $('.menu .menu__list').eq(0);
  var tv = menu.find('[data-action="tv"]').first();

  if (!menu.length || !tv.length) {
    if ((attempt || 0) < 20) {
      setTimeout(function() {
        addLampacDoramaMenuButton((attempt || 0) + 1);
      }, 500);
    }

    return;
  }

  if (menu.find('[data-action="dorama"]').length) {
    window.lampac_dorama_menu_ready = true;
    return;
  }

  var button = $("<li class=\"menu__item selector\" data-action=\"dorama\">\n        <div class=\"menu__ico\"><img src=\"./img/icons/menu/tv.svg\" /></div>\n        <div class=\"menu__text\">Дорамы</div>\n    </li>");

  button.on('hover:enter', function() {
    openLampacDoramaMenu();
  });

  tv.after(button);
  window.lampac_dorama_menu_ready = true;
}


/* Контекстное меню в online.js
window.lampac_online_context_menu = {
  push: function(menu, extra, params) {
    menu.push({
      title: 'TEST',
      test: true
    });
  },
  onSelect: function onSelect(a, params) {
    if (a.test)
      console.log(a);
  }
};
*/

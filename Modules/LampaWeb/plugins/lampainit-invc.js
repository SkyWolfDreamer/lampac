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

function getLampacDoramaAnchor() {
  var items = $('.menu .menu__item');
  var tv = items.filter('[data-action="tv"]').first();

  if (tv.length) return tv;

  return items.filter(function() {
    return $(this).find('.menu__text').text().trim() == 'Сериалы';
  }).first();
}

function bindLampacDoramaButton(button) {
  button.off('hover:enter.lampac-dorama click.lampac-dorama');
  button.on('hover:enter.lampac-dorama click.lampac-dorama', function() {
    openLampacDoramaMenu();
  });
}

function createLampacDoramaButton() {
  var button = $("<li class=\"menu__item selector\" data-action=\"dorama\">\n        <div class=\"menu__ico\"><img src=\"./img/icons/menu/tv.svg\" /></div>\n        <div class=\"menu__text\">Дорамы</div>\n    </li>");

  bindLampacDoramaButton(button);

  return button;
}

function addLampacDoramaMenuButton(attempt) {
  var anchor = getLampacDoramaAnchor();

  if (!anchor.length) {
    if ((attempt || 0) < 40) {
      setTimeout(function() {
        addLampacDoramaMenuButton((attempt || 0) + 1);
      }, 250);
    }

    return;
  }

  var existing = $('.menu .menu__item[data-action="dorama"]');
  var button = existing.first();

  existing.not(button).remove();

  if (!button.length) {
    button = createLampacDoramaButton();
  } else {
    bindLampacDoramaButton(button);
  }

  if (!button.prev().is(anchor)) {
    anchor.after(button.detach());
  }

  if ((attempt || 0) < 12) {
    setTimeout(function() {
      addLampacDoramaMenuButton((attempt || 0) + 1);
    }, 500);
  }
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

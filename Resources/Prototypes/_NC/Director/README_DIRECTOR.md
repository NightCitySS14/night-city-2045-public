# Документация системы «Живого Мира» (Global Director System)

Система **Global Director** — это автономный «рассказчик», который управляет динамическими сценариями. События теперь не просто случаются, а развиваются как нелинейные истории (State Machine), реагируя на мир и игроков.

---

## 1. Глобальный Конфигуратор (Мозг Директора)
Файл: `Resources/Prototypes/_NC/Director/gamerule.yml`

**Внимание:** `GlobalDirector` — это игровое правило (Game Rule). Его **не нужно** спавнить на карте вручную. Он запускается автоматически в начале раунда или админом. Он существует как невидимая системная сущность.

```yaml
- type: entity                 # Объявление новой сущности
  id: GlobalDirector           # Уникальный ID для системы (используется в коде)
  parent: BaseGameRule         # Наследование базовой логики игровых правил
  components:                  # Список компонентов, прикрепленных к этой сущности
  - type: GlobalDirector       # Логический компонент Директора
    minDelay: 300              # Минимальное время покоя между событиями (в секундах)
    maxDelay: 600              # Максимальное время покоя между событиями (в секундах)
    defaultAnnouncerId: "Director" # Имя отправителя в чате (по умолчанию "Глобальный Директор")
    announcementColor: "#00ffff"   # Цвет текста объявлений Директора (Cyan)
```

---

## 2. Описание Сценария (DirectorEvent)
Файл: `Resources/Prototypes/_NC/Director/*.yml`

### Заголовок события (Header)
```yaml
- type: directorEvent          # Указываем тип прототипа
  id: TestEvent                # ID для спавна через консоль (startdirectorevent TestEvent)
  name: "Тестовое событие"      # Понятное название для логов
  weight: 10                   # Шанс выпадения (чем выше число, тем чаще выбирается)
  announcerId: "Arasaka"       # Имя диктора для этого сценария
  announcementColor: "red"     # Цвет текста для этого сценария
  startPhase: "Initial"        # С какой фазы начинать
```

### Структура Фазы (Phases)
Каждая фаза — это отдельный «слой» истории.

```yaml
  phases:                      # Словарь всех фаз
    Initial:                   # ID текущей фазы
      name: "Подготовка"       # Название для отладки
      duration: 300            # Таймер жизни фазы в секундах
      announcement: "msg-id"   # Сообщение в чат в начале фазы
      locationTag: "Hidden"    # Искать на карте DirectorSpawnPoint с этим тегом.
                               # "Hidden" - за углом, "Alley" - в подворотне.
                               # Эти точки расставляет маппер в редакторе!
      aiDomain: "Tactical"     # Установить HTN-домен всем ботам события в этой фазе
      cleanup: false           # Удалить сущности фазы при выходе из неё
      spawns:                  # Список групп для появления
        - prototype: MobHuman  # ID прототипа NPC
          faction: NCBandit    # Фракция из ai_factions.yml (NCBandit, NCMilitech и т.д.)
          amount: 2            # Количество (по умолчанию 1)
      triggers:                # Условия для мгновенного перехода
        - type: MobKilled      # Смерть заспавненного NPC
          target: MobHuman     # Считать смерти только этого типа
          count: 2             # Нужно 2 смерти
      nextPhases:              # Куда идти дальше
        Combat: 70             # 70% шанс перейти в 'Combat'
        Police: 30             # 30% шанс перейти в 'Police'
```

---

## 3. Типы Триггеров (Triggers)

*   `type: MobKilled` — Смерть NPC события.
*   `type: EntityDestroyed` — Уничтожение объекта (турели, ящика).

---

## 4. Пространственная привязка (Mapping)

1. Маппер ставит сущность `DirectorSpawnPoint`.
2. В компоненте пишет `locationTag: "MyTag"`.
3. В YAML события пишет `locationTag: "MyTag"`.
4. Директор выберет одну случайную точку с этим тегом.

---

## 5. Локализация (Locale)
Файл: `Resources/Locale/ru-RU/_NC/Director/director.ftl`

```ftl
announcer-Arasaka-name = СБ Арасака
event-start-msg = Группа зачистки в пути
```

---

## 7. Как активировать Директора (Activation)

Сам по себе прототип `GlobalDirector` в `gamerule.yml` — это просто описание. Чтобы система начала работать, её нужно запустить одним из способов:

### А. Автоматически (через Пресеты)
Добавьте ID правила в список `rules` вашего игрового пресета (например, в `roundstart.yml`):
```yaml
- type: gamePreset
  id: MyPreset
  rules:
  - GlobalDirector  # Теперь Директор будет запускаться сам при старте раунда
```

### Б. Вручную (через Консоль Админа)
Если раунд уже идет, введите команду:
`addgamerule GlobalDirector`

---

## 8. Команды управления

*   `startdirectorevent <ID>` — Принудительный старт.
*   `advancedirectorevent <UID>` — Переход к следующей фазе.
*   `canceldirectorevent <UID>` — Жесткая остановка.

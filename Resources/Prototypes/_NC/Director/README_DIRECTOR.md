# Документация системы «Живого Мира» (Global Director System)

Система Global Director предназначена для создания динамического окружения в Cyberpunk RED. Она управляет «событиями», которые развиваются по фазам (слоям), имитируя жизнь города независимо от игроков.

---

## 1. Основные сущности

### GlobalDirector (Геймрул)
Это «мозг» системы. Он настраивается через прототип сущности в `Resources/Prototypes/_NC/Director/gamerule.yml`.

**Параметры:**
* `minDelay` / `maxDelay`: Задержка между попытками запустить новое случайное событие (в секундах).
* `defaultAnnouncerId`: Имя диктора по умолчанию (например, "Director").
* `announcementColor`: Цвет сообщений по умолчанию.

### DirectorEvent (Прототип события)
Описывает сценарий. Файлы лежат в `Resources/Prototypes/_NC/Director/`.

**Структура прототипа:**
```yaml
- type: directorEvent
  id: MyCoolEvent          # Уникальный ID
  name: "Название для админов"
  weight: 10               # Вес для рандома (чем больше, тем чаще выпадает)
  announcerId: "NCPD"      # (Опционально) Кто отправляет сообщения
  announcementColor: red   # (Опционально) Цвет сообщений
  phases:                  # Список фаз события
    - name: "Phase 1"      # Техническое имя фазы
      duration: 300        # Длительность в секундах (авто-переход)
      announcement: "loc-id" # ID локализации сообщения
      spawns:              # Список сущностей для спавна в начале фазы
        - MobHuman
      triggers:            # Условия для ДОСРОЧНОГО перехода к след. фазе
        - type: MobKilled  # Тип триггера
          target: MobHuman # (Опционально) Если убит именно этот прототип
```

---

## 2. Триггеры (Triggers)

Триггеры позволяют событию реагировать на действия игроков:

1. **MobKilled**: Срабатывает, когда любой NPC, заспавненный этим событием, умирает.
2. **EntityDestroyed**: Срабатывает, когда объект события (например, ящик или турель) уничтожен.
3. **Timer (Duration)**: Если триггеры не сработали, фаза завершится сама по истечении `duration`.

---

## 3. Как добавить новое событие (Пример)

Допустим, мы хотим сделать «Корпоративную засаду».

### Шаг 1: Создаем прототип
Создайте файл `Resources/Prototypes/_NC/Director/corp_ambush.yml`:

```yaml
- type: directorEvent
  id: CorpAmbush
  name: "Корпоративная засада"
  weight: 5
  announcerId: "Arasaka"
  announcementColor: "#ff4444"
  phases:
    # Фаза 1: Подготовка
    - name: "Deployment"
      duration: 120
      announcement: corp-ambush-start
      spawns:
        - MobHumanSyndicateAgent # Спавним агентов
    
    # Фаза 2: Бой
    - name: "Active Combat"
      duration: 600
      announcement: corp-ambush-combat
      triggers:
        - type: MobKilled # Перейдем к финалу, если кого-то убьют
    
    # Фаза 3: Отступление
    - name: "Retreat"
      duration: 60
      announcement: corp-ambush-end
```

### Шаг 2: Добавляем локализацию
В файл `Resources/Locale/ru-RU/_NC/Director/director.ftl`:

```ftl
announcer-Arasaka-name = Служба Безопасности Arasaka
corp-ambush-start = Внимание: Зафиксировано развертывание оперативной группы в секторе.
corp-ambush-combat = Контакт подтвержден. Применяется летальная сила.
corp-ambush-end = Операция завершена. Группа зачистки отступает.
```

---

## 4. Полезные команды для тестов

* `startdirectorevent <ID>` — Запустить событие принудительно (например: `startdirectorevent CorpAmbush`).
* `advancedirectorevent <UID>` — Мгновенно переключить фазу (UID события можно найти в логах сервера).

## 5. Точки спавна
Чтобы событие знало, где спавнить NPC, на карте должны быть расставлены сущности `DirectorSpawnPoint`. Система выбирает случайную точку из всех доступных на карте.

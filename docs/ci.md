# Проверки CI

Workflow находится в [build-and-deploy.yaml](../.github/workflows/build-and-deploy.yaml).
Он проверяет PR в `development` и поддерживает ручной запуск через `workflow_dispatch`.
Автоматические проверки выполняются до merge. Workflow собирает Docker images для
тестирования; публикация images и развёртывание приложения в него не входят.

## Какие проверки запускаются

| Check в GitHub | Что проверяет |
| --- | --- |
| `Workflow validation` | actionlint: YAML, выражения GitHub Actions и shell-команды workflow |
| `build` | .NET restore и Release build, соответствие EF snapshot, unit и integration tests, сборка API и seeder images |
| `Frontend checks` | `npm ci`, строгий ESLint, TypeScript и production build Next.js |
| `Monitoring smoke test` | конфигурации мониторинга, метрики, traces и provisioning Grafana на изолированном Compose project |

Первые три jobs выполняются параллельно. Мониторинг ждёт успешного `build` и получает
собранные им images через artifact. Имя `build` сохранено для существующих настроек
обязательных проверок репозитория.

Ветка `ci-hardening` создана от `development`. Изменения незавершённой ветки
`observability-logs` в неё не перенесены. После merge логирования тот же monitoring
job будет выполнять расширенный smoke сценарий из этой ветки.

## Frontend

Версия Node закреплена в [client/.nvmrc](../client/.nvmrc); CI читает её через
`setup-node`. npm используется из выбранного дистрибутива Node. Зависимости
устанавливаются по `package-lock.json` командой `npm ci`.

Из каталога `client`:

```powershell
npm ci --no-audit --no-fund
npm run lint
npm run typecheck
npm run build
```

`lint` завершится с ошибкой даже при предупреждениях (`--max-warnings 0`).
`typecheck` сначала выполняет `next typegen`, чтобы проверка TypeScript учитывала
сгенерированные типы маршрутов, затем запускает `tsc --noEmit`.

Для сборки не нужна работающая API: `NEXT_PUBLIC_API_URL` в CI равен
`http://localhost:5227`. Это публичное тестовое значение. При сборке для другого
окружения URL задаётся отдельно. `NEXT_TELEMETRY_DISABLED=1` отключает телеметрию
Next.js. Текущий `next/font/google` загружает шрифты во время build, поэтому ему
нужен доступ к серверам Google.

## Backend

SDK выбирается из `global.json`, EF CLI — из `.config/dotnet-tools.json`.
После restore и Release build выполняется
`dotnet ef migrations has-pending-model-changes`: изменения модели без миграции
останавливают проверку.

Unit и integration tests запускаются отдельными шагами с `--no-build` и
`--no-restore`. Integration tests используют Docker и изолированные Testcontainers.
TRX сохраняются в `.local/test-results/unit` и `.local/test-results/integration`.

Два Docker build используют Buildx, загружают images в локальный Docker daemon
runner и передают их monitoring job. Smoke test мониторинга запускается на тех
images, которые собраны в текущем run.

## Кеши, таймауты и отмена запусков

- npm cache содержит скачанные пакеты; его ключ учитывает Node и `package-lock.json`.
- NuGet cache содержит пакеты; ключ учитывает проекты, SDK, конфигурацию и EF CLI.
  Restore выполняется и при попадании в кеш.
- Docker использует GitHub Actions cache с отдельными scopes `product-api` и
  `product-seeder`. Ошибка сохранения кеша не отменяет успешную сборку.
- Для jobs заданы пределы: workflow validation — 5 минут, backend — 30 минут,
  frontend и monitoring — по 15 минут.
- Новый запуск для того же PR отменяет предыдущий. Для ручных запусков группа
  определяется веткой.

## Права и диагностические artifacts

Workflow задаёт `permissions: contents: read`; checkout не сохраняет Git credentials.
Actions закреплены по commit SHA, image actionlint — по digest. Их версии обновляются
явным изменением workflow.

| Artifact | Содержимое | Хранение |
| --- | --- | --- |
| `backend-test-results` | TRX unit и integration tests | 7 дней |
| `frontend-diagnostics` | логи npm/lint/typecheck/build и JSON diagnostics Next.js | 7 дней |
| `monitoring-diagnostics` | состояние Compose, логи и итоговый JSON smoke test | 7 дней |
| `monitoring-images` | собранные API и seeder images для следующего job | 1 день |

Диагностические artifacts загружаются с `if: always()`, включая неуспешные проверки.
Artifact images загружается только после успешной сборки. Пути перечислены явно:
локальные `.env` и весь каталог `.local` не загружаются.

## Защита `development`

Workflow запускает проверки, а запрет merge задаётся отдельно в GitHub Rulesets.
Действующий ruleset `6493187` обновлён через GitHub API. Его условие
`~DEFAULT_BRANCH` сейчас соответствует `development`.

- Изменения проходят через PR.
- Все четыре checks из таблицы обязательны; источник — GitHub Actions.
- Перед merge ветка PR должна учитывать актуальный `development`.
- Сохранены запреты удаления ветки и force push.
- Обязательных approvals от другого участника нет; список bypass actors пуст.

Новые checks появятся при запуске PR с обновлённым workflow; до этого GitHub
будет ожидать их.
Настройки ruleset действуют независимо от этого коммита. При переименовании checks
нужно синхронно обновлять их имена в ruleset.

Ручной запуск доступен в Actions, когда версия workflow с `workflow_dispatch`
присутствует в default branch репозитория; затем можно выбрать нужную ветку.

## Следующие отдельные задачи

- Браузерные тесты приглашения, входа, ролей и создания facility.
- Проверка уязвимостей npm/NuGet и автоматические PR обновлений зависимостей.

## Документация инструментов

- [Node version file и npm cache](https://github.com/actions/setup-node/blob/main/docs/advanced-usage.md).
- [Docker cache в GitHub Actions](https://docs.docker.com/build/ci/github-actions/cache/).
- [Type generation Next.js](https://nextjs.org/docs/app/api-reference/cli/next#next-typegen-options).
- [Concurrency GitHub Actions](https://docs.github.com/en/actions/how-tos/write-workflows/choose-when-workflows-run/control-workflow-concurrency).
- [Ручной запуск workflow](https://docs.github.com/en/actions/how-tos/manage-workflow-runs/manually-run-a-workflow).

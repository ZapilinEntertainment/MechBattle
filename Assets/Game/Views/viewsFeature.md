# Views
Проект предполагает смешанный подход к отрисовке игровых объектов - как обычные renderers (Mono) так и GPU-отрисовка (Virtual). Поэтому все отображения в игровом мире идут через операции с IViewContainer, индексированные в ViewContainersManager. IViewContainers указываются на сущностях по id. Конечный потребитель производит с ними операции через IViewContainerHandler, а фабрики пользуются ViewFactory и ViewApplier.

# View контейнеры
IViewContainer не содержат логики, все операции с ними должны осуществлять соответствующие хэндлеры (IContainerHandler для внешних потребителей, а сама реализация в ISpecifiedViewContainerHandler).
Это пустые poolable объекты, реализующие IViewContainer. Они получаются через IViewContainersManager, каждый вызов регистрируется отдельным id, который и указывается в компоненте сущности. Сейчас их два вида - Mono (пустой transform объект) и Virtual (C# объект).

# Создание
View добавляется двумя видами - либо создается сразу сущностью через ViewFactory, либо визуальная часть добавляется через ViewApplier. Оба класса делегируют создание / подготовку view специализированным апплаерам, получаемым из ViewApplicationStrategiesList.

# View-стратегии
 В ViewFeatureModule стратегии визуализации регистрируются в методе RegisterViewMode, содержащем 2 generic аргумента - IViewApplyStrategy-объект занимается созданием и подготовкой, а ISpecifiedViewContainerHandler отвечает за взаимодействие и изменение view.
 На данный момент в проекте доступно 4 стратегии:
1) Scene Mono - создает вью-сущность из представленного на сцене объекта (например, здания. Сам объект является одноразовым IViewContainer.
2) Delayed Mono - запрашивает пустой MonoViewContainer, добавляет компонент запроса нужной модели. Как только модель загружена, система ViewRequestsHandleSystem добавит вью в контейнер (поле ContainedView) и выполнит другие операции с вью (см. раздел Finalizing View)
3) Virtual  - простая вью для virtual renderer (отрисовка на GPU). Содержит только 1 компонент VirtualViewComponent на самой сущности. Сейчас применяется для простых объектов (снаряды) и фактически не имеет контейнера вью (и соответственно не реализует какого-либо связанного функционала)
4) Complex Virtual - несколько связанных между собой виртуальных view. Запрашивает пустой VirtualViewContainer и создает родительскую сущностью view, дочерние объекты создает через апплаер обычных vritual view и записывает их сущности в контейнер.

# ViewSynchronizationApplier
Апплаер, созданный чтобы связать view и entity вызовом одного метода. Аргументы метода Apply:
- applyViewPosition - нужно ли скопировать мировую позицию view на entity, или же наоборот, установить view позицию entity
- finalViewApplicationRequired - нужно ли зарегистрировать коллайдеры view и применить на нее расцветку игрока. Обратите внимание - для delayed версии в этот момент view  это пустой view container, FinalApplication для них вызывается после загрузки и установки реальной view

# FinalViewFunctionalApplier
Апплаер, завершающий регистрацию view как части игрового мира
- Регистрирует связанный с view коллайдер на ее владельца
- Устанавливает цвет игрока для view, которые могут быть окрашены

# View Parts
Комплексные вью могу содержать несколько частей, каждая из которых маркируется отдельным ключом ViewPartKey. Например, модель меха грузится как единый префаб, но после инстанса внутренние системы разбирают ее на отдельные части по ключам, отдавая управление каждой соответствующей частью уполномоченным сущностям.

# Delayed Mono Views

 ## Пайплайн
  Unity Views существуют как отдельные объекты в игровом мире, их позиции синхронизуются с компонентами PositionComponent \ RotationComponent через TransformAccess (TransformAccessManager / TransformsSyncSystem). Для мгновенного создания views и отсутствия статтеров установлена следующая схема для delayed mono:
 1) IVIewContainesManager выдает transform-пустышку (MonoViewContainer). Это poolable объект, которые принимает внутрь себя настоящий view сущности. Во ViewKeyComponent записывается идентификатор ассета, который указан в конфиге сущности
 2) Для загрузки нужной модели на сущность вешается ViewLoadRequestTag
 3) ViewRequestsHandleSystem создает и проверяет ViewProviders - объекты, которые хранят данные о состоянии загрузки и сами загруженные префабы. Как только провайдер готов предоставить ассет, интанциируем настоящий view и присваиваем его view container-у.
 4) в кадр может вызваться только ограниченное количество сущностей (GameConstants.MAX_VIEW_INSTANTIATIONS_PER_FRAME)
 
 
 ## ViewProviders
 ViewProvidersFactory выдает провайдеров - объекты, которые предоставляют по возможности загружаемый addressable ассет. Если загружаемая view поддерживает пуллинг, то фабрика создаст ViewPool, иначе - обычный ViewProvider. Пока идет загрузка, фабрика выдает провайдер-заглушку, у которого всегда IsReady == false
 
 ## Очистка
 Прямо перед диспозом scene scope модуль освобождает все view container (ViewFeatureModule.OnWorldDispose)
 
 
 
# Загрузка ресурсов сессии
При подготовке SessionScope модуль загружает следующие ресурсы:
- Материалы с MATERIAL_LABEL и SESSION_PRELOAD_LABEL
- Конфиги простых (VirtualViewConfigurationObject) и сложных (ComplexVirtualViewConfigurationObject) виртуальных объектов по VIRTUAL_VIEW_CONFIG_LABEL и SESSION_PRELOAD_LABEL
- Меши: встроенные в редактор, простые (1 меш внутри, записывается по PrimaryKey) и комплексные (несколько мешей внутри, идентификаторы через MESH[SUBMESH_NAME]) с лейблами MESH_LABEL и SESSION_PRELOAD_LABEL
- Материалы кэшируются в MaterialsDepot, меши - в MeshesDepot, конфиги в VirtualViewConfigsList и ComplexVirtualViewConfigsList. Depots реализованы как AssetDictionary / DynamicAssetDictionary - в них возможно получить ассет как по строковому ключу, так и по сгенерированному int-ключу (указывается в компонентах)
- Отдельно существует ColouredMaterialsDepot, который, опираясь на MaterialsDepot хранит версии перекрашенные версии материалов для текущей сцены (потом стирает их).
 
====
 
 # Не реализовано
 - куллинг виртуальных объектов
  - добавление коллайдеров до загрузки delayed mono view
  - добавление коллайдеров и физики на виртуальные объекты
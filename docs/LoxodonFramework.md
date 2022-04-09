目录
---

- [功能介绍](#功能介绍)
  - [上下文（Context）](#上下文context)
    - [应用上下文（ApplicationContext）](#应用上下文applicationcontext)
    - [玩家上下文（PlayerContext）](#玩家上下文playercontext)
    - [其它上下文（Context）](#其它上下文context)
  - [服务容器](#服务容器)
    - [服务注册器(IServiceRegistry)](#服务注册器iserviceregistry)
    - [服务定位器(IServiceLocator)](#服务定位器iservicelocator)
    - [服务Bundle(IServiceBundle)](#服务bundleiservicebundle)
  - [应用配置（Preference）](#应用配置preference)
  - [消息系统(Messenger)](#消息系统messenger)
  - [可观察的对象(Observables)](#可观察的对象observables)
  - [数据绑定(Databinding)](#数据绑定databinding)
    - [数据绑定示例](#数据绑定示例)
    - [绑定模式](#绑定模式)
    - [类型转换器(IConverter)](#类型转换器iconverter)
    - [绑定类型](#绑定类型)
    - [Command Parameter](#command-parameter)
    - [Scope Key](#scope-key)
    - [绑定的生命周期](#绑定的生命周期)
    - [注册属性和域的访问器](#注册属性和域的访问器)
  - [UI框架](#ui框架)
    - [动态变量集(Variables)](#动态变量集variables)
    - [UI视图定位器(IUIViewLocator)](#ui视图定位器iuiviewlocator)
    - [UI视图动画(Animations)](#ui视图动画animations)
    - [UI控件](#ui控件)
    - [视图、窗口和窗口管理器](#视图-窗口和窗口管理器)
    - [交互请求(InteractionRequest)](#交互请求interactionrequest)
    - [交互行为(InteractionAction)](#交互行为interactionaction)
    - [集合与列表视图的绑定](#集合与列表视图的绑定)
    - [数据绑定与异步加载精灵](#数据绑定与异步加载精灵)

## 快速入门

创建一个视图，左侧显示一个账号信息，右侧是一个表单，通过提交按钮可以修改左侧的账号信息，现在我们通过框架的视图和数据绑定功能来演示我们是如何做的。界面如下图：

![](images/DatabindingExample_01.png)

### C# 示例

在一个UI视图的根对象上添加视图脚本组件DatabindingExample，并且将UI控件赋值到对应的属性上,这个示例中属性都是通过C#硬编码来定义的，当然你也可以使用动态的属性表VariableArray来动态定义属性，具体可以看Lua的例子，配置好属性后如下图所示。

![](images/DatabindingExample_03.png)

下面请看代码，我们是如果来定义视图模型和视图脚本的，又是怎么样来绑定视图到视图模型的。
```csharp
    /// <summary>
    /// 账号子视图模型
    /// </summary>
    public class AccountViewModel : ObservableObject
    {
        private int id;
        private string username;
        private string password;
        private string email;
        private DateTime birthday;
        private readonly ObservableProperty<string> address = new ObservableProperty<string>();

        public int ID
        {
            get { return this.id; }
            set { this.Set<int>(ref this.id, value, "ID"); }
        }

        public string Username
        {
            get { return this.username; }
            set { this.Set<string>(ref this.username, value, "Username"); }
        }

        public string Password
        {
            get { return this.password; }
            set { this.Set<string>(ref this.password, value, "Password"); }
        }

        public string Email
        {
            get { return this.email; }
            set { this.Set<string>(ref this.email, value, "Email"); }
        }

        public DateTime Birthday
        {
            get { return this.birthday; }
            set { this.Set<DateTime>(ref this.birthday, value, "Birthday"); }
        }

        public ObservableProperty<string> Address
        {
            get { return this.address; }
        }
    }


    /// <summary>
    /// 数据绑定示例的视图模型
    /// </summary>
    public class DatabindingViewModel : ViewModelBase
    {
        private AccountViewModel account;
        private bool remember;
        private string username;
        private string email;
        private ObservableDictionary<string, string> errors = new ObservableDictionary<string, string>();

        public AccountViewModel Account
        {
            get { return this.account; }
            set { this.Set<AccountViewModel>(ref account, value, "Account"); }
        }

        public string Username
        {
            get { return this.username; }
            set { this.Set<string>(ref this.username, value, "Username"); }
        }

        public string Email
        {
            get { return this.email; }
            set { this.Set<string>(ref this.email, value, "Email"); }
        }

        public bool Remember
        {
            get { return this.remember; }
            set { this.Set<bool>(ref this.remember, value, "Remember"); }
        }

        public ObservableDictionary<string, string> Errors
        {
            get { return this.errors; }
            set { this.Set<ObservableDictionary<string, string>>(ref this.errors, value, "Errors"); }
        }

        public void OnUsernameValueChanged(string value)
        {
            Debug.LogFormat("Username ValueChanged:{0}", value);
        }

        public void OnEmailValueChanged(string value)
        {
            Debug.LogFormat("Email ValueChanged:{0}", value);
        }

        public void OnSubmit()
        {
            if (string.IsNullOrEmpty(this.Username) || !Regex.IsMatch(this.Username, "^[a-zA-Z0-9_-]{4,12}$"))
            {
                this.errors["errorMessage"] = "Please enter a valid username.";
                return;
            }

            if (string.IsNullOrEmpty(this.Email) || !Regex.IsMatch(this.Email, @"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$"))
            {
                this.errors["errorMessage"] = "Please enter a valid email.";
                return;
            }

            this.errors.Clear();
            this.Account.Username = this.Username;
            this.Account.Email = this.Email;
        }
    }


    /// <summary>
    /// 数据绑定示例视图
    /// </summary>
    public class DatabindingExample : UIView
    {
        public Text title;
        public Text username;
        public Text password;
        public Text email;
        public Text birthday;
        public Text address;
        public Text remember;

        public Text errorMessage;

        public InputField usernameEdit;
        public InputField emailEdit;
        public Toggle rememberEdit;
        public Button submit;

        protected override void Awake()
        {
            //获得应用上下文
            ApplicationContext context = Context.GetApplicationContext();

            //启动数据绑定服务
            BindingServiceBundle bindingService = new BindingServiceBundle(context.GetContainer());
            bindingService.Start();

            //初始化本地化服务
            CultureInfo cultureInfo = Locale.GetCultureInfo();
            var provider = new DefaultDataProvider("LocalizationTutorials", new XmlDocumentParser())
            Localization.Current = Localization.Create(provider, cultureInfo);

        }

        protected override void Start()
        {
            //创建账号子视图
            AccountViewModel account = new AccountViewModel()
            {
                ID = 1,
                Username = "test",
                Password = "test",
                Email = "yangpc.china@gmail.com",
                Birthday = new DateTime(2000, 3, 3)
            };
            account.Address.Value = "beijing";

            //创建数据绑定视图
            DatabindingViewModel databindingViewModel = new DatabindingViewModel()
            {
                Account = account
            };

            //获得数据绑定上下文
            IBindingContext bindingContext = this.BindingContext();

            //将视图模型赋值到DataContext
            bindingContext.DataContext = databindingViewModel;

            //绑定UI控件到视图模型
            BindingSet<DatabindingExample, DatabindingViewModel> bindingSet;
            bindingSet = this.CreateBindingSet<DatabindingExample, DatabindingViewModel>();

            //绑定左侧视图到账号子视图模型
            bindingSet.Bind(this.username).For(v => v.text).To(vm => vm.Account.Username).OneWay();
            bindingSet.Bind(this.password).For(v => v.text).To(vm => vm.Account.Password).OneWay();
            bindingSet.Bind(this.email).For(v => v.text).To(vm => vm.Account.Email).OneWay();
            bindingSet.Bind(this.remember).For(v => v.text).To(vm => vm.Remember).OneWay();
            bindingSet.Bind(this.birthday).For(v => v.text).ToExpression(vm => string.Format("{0} ({1})",
             vm.Account.Birthday.ToString("yyyy-MM-dd"), (DateTime.Now.Year - vm.Account.Birthday.Year))).OneWay();
            bindingSet.Bind(this.address).For(v => v.text).To(vm => vm.Account.Address).OneWay();

            //绑定右侧表单到视图模型
            bindingSet.Bind(this.errorMessage).For(v => v.text).To(vm => vm.Errors["errorMessage"]).OneWay();
            bindingSet.Bind(this.usernameEdit).For(v => v.text, v => v.onEndEdit).To(vm => vm.Username).TwoWay();
            bindingSet.Bind(this.usernameEdit).For(v => v.onValueChanged).To<string>(vm => vm.OnUsernameValueChanged);
            bindingSet.Bind(this.emailEdit).For(v => v.text, v => v.onEndEdit).To(vm => vm.Email).TwoWay();
            bindingSet.Bind(this.emailEdit).For(v => v.onValueChanged).To<string>(vm => vm.OnEmailValueChanged);
            bindingSet.Bind(this.rememberEdit).For(v => v.isOn, v => v.onValueChanged).To(vm => vm.Remember).TwoWay();
            bindingSet.Bind(this.submit).For(v => v.onClick).To(vm => vm.OnSubmit);
            bindingSet.Build();

            //绑定标题,标题通过本地化文件配置
            BindingSet<DatabindingExample> staticBindingSet = this.CreateBindingSet<DatabindingExample>();
            staticBindingSet.Bind(this.title).For(v => v.text).To(() => Res.databinding_tutorials_title).OneTime();
            staticBindingSet.Build();
        }
    }
```

## 功能介绍

### 上下文（Context）
在很多框架中，我们应该经常看到上下文这个概念，它可以说就是与当前代码运行相关的一个环境，你能在上下文中提供了当前运行需要的环境数据或者服务等。在这里，我根据游戏开发的特点，我提供了应用上下文（ApplicationContext）、玩家上下文（PlayerContext），同时也支持开发人员根据自己的需求来创建其他的上下文。

在上下文中，我创建了一个服务容器（有关服务容器的介绍请看下一章节）来存储与当前上下文相关的服务，同时创建了个字典来存储数据。通过上下文的Dispose()，可以释放所有在上下文容器中注册的服务。**但是需要注意的是，服务必须继承System.IDisposable接口，否则不能自动释放。**

#### 应用上下文（ApplicationContext）

应用上下文是一个全局的上下文，它是单例的，它主要存储全局共享的一些数据和服务。所有的基础服务，比如视图定位服务、资源加载服务，网络连接服务、本地化服务、配置文件服务、Json/Xml解析服务、数据绑定服务等等，这些在整个游戏中都可能使用到的基础服务都应该注册到应用上下文的服务容器当中，可以通过应用上下文来获得。
```csharp
    //获得全局的应用上下文
    ApplicationContext context = Context.GetApplicationContext();

    //获得上下文中的服务容器
    IServiceContainer container = context.GetContainer();

    //初始化数据绑定服务，这是一组服务，通过ServiceBundle来初始化并注册到服务容器中
    BindingServiceBundle bundle = new BindingServiceBundle(context.GetContainer());
    bundle.Start();

    //初始化IUIViewLocator，并注册到容器
    container.Register<IUIViewLocator>(new ResourcesViewLocator ());

    //初始化本地化服务，并注册到容器中
    CultureInfo cultureInfo = Locale.GetCultureInfo();
    var dataProvider = new ResourcesDataProvider("LocalizationExamples", new XmlDocumentParser());
    Localization.Current = Localization.Create(dataProvider, cultureInfo);
    container.Register<Localization>(Localization.Current);

    //从全局上下文获得IUIViewLocator服务
    IUIViewLocator locator = context.GetService<IUIViewLocator>();

    //从全局上下文获得本地化服务
    Localization localization = context.GetService<Localization>();


#### 玩家上下文（PlayerContext）

玩家上下文是只跟当前登录的游戏玩家相关的上下文，比如一个游戏玩家Clark登录游戏后，他在游戏中的基本信息和与之相关的服务，都应该存储在玩家上下文中。比如背包服务，它负责拉取和同步玩家的背包数据，缓存了玩家背包中的武器、装备、道具等等，它只与当前玩家有关，当玩家退出登录切换账号时，这些数据都应该被清理和释放。我们使用了玩家上下文来存储这些服务和数值时，只需要调用PlayerContext.Dispose()函数，就可以释放与当前玩家有关的所有数据和服务。

玩家上下文中默认继承了全局上下文的所有服务和属性，所以通过玩家上下文可以获取到所有在全局上下文中的服务和数据，当玩家上下文注册了与全局上下文中Key值相同的服务或者是属性时，它会在玩家上下文中存储，不会覆盖全局上下文中存储的数据，当通过Key访问时，优先返回玩家上下文中的数据，只有在玩家上下文中找不到时才会去全局上下文中查找。

    //为玩家clark创建一个玩家上下文
    PlayerContext playerContext = new PlayerContext("clark");

    //获得玩家上下文中的服务容器
    IServiceContainer container = playerContext.GetContainer();

    //将角色信息存入玩家上下文
    playerContext.Set("roleInfo", roleInfo);

    //初始化背包服务，注册到玩家上下文的服务容器中
    container.Register<IKnapsackService>(new KnapsackService());

    //从通过玩家上下文获得在全局上下文注册的IViewLocator服务
    IUIViewLocator locator = playerContext.GetService<IUIViewLocator>();

    //从通过玩家上下文获得在全局上下文注册的本地化服务
    Localization localization = playerContext.GetService<Localization>();

    //当用户clark退出登录时，注销玩家上下文，自动注销所有注册在当前玩家上下文中的服务。
    playerContext.Dispose();
```

#### 其它上下文（Context）

一般来说，在很多游戏开发中，我们只需要全局上下文和玩家上下文就足以满足要求，但是在某些情况下，我们还需要一个上下文来存储环境数据，比如在MMO游戏中，进入某个特定玩法的副本，那么我就需要为这个副本创建一个专属的上下文，当副本中的战斗结束，退出副本时，则销毁这个副本上下文来释放资源。
```csharp
    //创建一个上下文，参数container值为null，在Context内部会自动创建
    //参数contextBase值为playerContext，自动继承了playerContext中的服务和属性
    Context context = new Context(null,playerContext);

    //获得上下文中的服务容器
    IServiceContainer container = context.GetContainer();

    //注册一个战斗服务到容器中
    container.Register<IBattleService>(new BattleService());
```
### 服务容器
在项目开始时，我曾调研过很多C#的控制反转和依赖注入（IoC/DI）方面的开源项目，开始是想用Zenject来做为服务的容器使用，后来因为考虑到移动项目中，内存和CPU资源都相当宝贵，不想再引入一个这么大的库来消耗内存，也不想因为反射导致的性能损失，而且强制用户使用IoC/DI也不太合适，毕竟不是所有人都喜欢，所以我就自己设计了一个简单的服务容器，来满足服务注册、注销、读取这些最基本的功能。

**注意：所有注册的服务，只有继承System.IDisposable接口，实现了Dispose函数，才能在IServiceContainer.Dispose()时自动释放。**

#### 服务注册器(IServiceRegistry)

服务注册负责注册和注销服务，它可以根据服务类型或者服务名称注册一个服务实例到容器中，也可以注册一个服务工厂到容器中，用户可以根据自己的需求来选择是否需要注册一个服务工厂，是创建一个单态的服务，还是每次都创建一个新的服务实例。
```csharp
    IServiceContainer container = ...
    IBinder binder = ...
    IPathParser pathParser = ...

    //注册一个类型为IBinder的服务到容器中,可以通过container.Resolve<IBinder>() 或者
    //container.Resolve("IBinder") 来访问这个服务，在容器中默认使用了typeof(IBinder).Name做为Key存储。   
    container.Register<IBinder>(binder);

    //如果需要注册多个IPathParser到容器中，请使用name参数区分
    //在取值时通过name参数取值，如：container.Resolve("parser")
    container.Register<IPathParser>("parser",pathParser);
    container.Register<IPathParser>("parser2",pathParser2);
```
#### 服务定位器(IServiceLocator)  

通过服务定位器可以获得服务，服务定位器可以根据服务名称或者类型来查询服务，当服务以类型的方式注册，则可以通过类型或者类型名来查找服务，当服务以特定的名称为Key注册，则只能通过服务名来查找服务。
```csharp
    IServiceContainer container = ...

    //IBinder服务在上段代码中，以类型方式注册，所以可以通过类型或者名称方式查询服务
    IBinder binder = container.Resolve<IBinder>()；//or container.Resolve("IBinder")

    //IPathParser在上段代码中以特定名称"parser"注册，则只能通过名称"parser"来查询服务
    IPathParser pathParser = container.Resolve("parser");
```
#### 服务Bundle(IServiceBundle)

ServiceBundle的作用是将一组相关的服务打包注册和注销，比如我的数据绑定服务，就是通过ServiceBundle.Start()方法一次性注册所有数据绑定有关的服务，当服务不在需要时，又可以通过ServiceBundle.Stop()方法来注销整个模块的所有服务（见下面的代码）。这在某些时候非常有用，比如启动和停止一个模块的所有服务。
```csharp
    //初始化数据绑定模块，启动数据绑定服务,注册服务
    BindingServiceBundle bundle = new BindingServiceBundle(context.GetContainer());
    bundle.Start();

    //停止数据绑定模块，注销所有数据绑定相关的服务
    bundle.Stop();
```

### 应用配置（Preference）
Perference可以说就是Unity3d的PlayerPrefs，只是我对PlayerPrefs的功能进行了扩展、补充和标准化。Perference除了可以存储boolean、int、 float、string等基本数据类型之外，还可以存储DateTime、Vector2、Vector3、Vector4、Color、Version，以及任何JsonUtility可以序列化的对象类型，甚至你可以自己自定义类型编码解码器（ITypeEncoder）来扩展任何你想存储的类型。Perference支持加密的方式存储数据，并且我实现了两种持久化的方式，第一种是将数据转换为string的方式存储在Unity3D的PlayerPrefs中。第二种是以二进制的方式存储在文件中，一般在项目测试时我都使用文件持久化的方式，因为我可以直接删除Application.persistentDataPath目录下的文件方便的删除配置。

Perference除了扩展以上功能外，我还扩展了配置的作用域，如同前文中的Context一样，同样包括全局的配置和玩家的配置，也同样支持某个局部模块的配置。全局配置可以用来存放当前资源更新的版本，最后登录的用户名等与应用相关的信息；玩家配置可以存在多个（如果在一台机器上有多个账户登录的话），可以存放具体某个玩家在本机的配置信息，如玩家在游戏中背景音乐、音效、画面质量、视距远近的设置等等。

下面跟随我的代码，我们来了解它是如何使用的。
```csharp
    //注册一个Preference的工厂，默认是PlayerPrefsPreferencesFactory工厂，只有使用File持久化才需要改为BinaryFilePreferencesFactory工厂
    Preferences.Register(new BinaryFilePreferencesFactory());

    //获得全局配置，如果不存在则自动创建
    Preferences globalPreferences = Preferences.GetGlobalPreferences();

    //存储当前资源更新后的数据版本
    globalPreferences.SetObject<Version>("DATA_VERSION",dataVersion);

    //存储游戏最后成功登录的用户名，下次启动游戏时自动填写在账号输入框中
    globalPreferences.SetString("username","clark");

    //数据修改后调用Save函数保存数据
    globalPreferences.Save();

    //根据key值"clark@zone5"获得配置，如果不存在则自动创建，这里的意思是获得游戏第5区名为clark的用户的配置信息
    //在Preferences.GetPreferences()函数中，name只是一个存取的Key，你可以完全按自己的意思组合使用。
    Preferences userPreferences Preferences.GetPreferences("clark@zone5");

    //设置游戏音乐、音效开关，并保存
    userPreferences.SetBool("Music_Enable",true);
    userPreferences.SetBool("Sound_Enable",true);
    userPreferences.Save();
```
在Preferences中，我虽然已支持了很多种的数据类型，但是总有些特殊需求我是无法满足的，那么你通过ITypeEncoder来扩展自己的类型；并且如果你对配置数据的安全性是有要求的，那么你也可以使用自己的密码来加密数据。
```csharp
    /// <summary>
    /// 自定义一个类型编码器
    /// </summary>
    public class ColorTypeEncoder : ITypeEncoder
    {
        private int priority = 900; //当一个类型被多个类型编码器支持时，优先级最高的有效(优先级在-999到999之间)

        public int Priority
        {
            get { return this.priority; }
            set { this.priority = value; }
        }

        public bool IsSupport(Type type)
        {
            if (type.Equals(typeof(Color)))
                return true;
            return false;
        }

        //将string类型转回对象类型
        public object Decode(Type type, string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            Color color;
            if(ColorUtility.TryParseHtmlString(value,out color))
                return color;

            return null;
        }

        //将对象转换为string来保存，因为PlayerPrefs只支持string类型的数据
        public string Encode(object value)
        {            
            return ColorUtility.ToHtmlStringRGBA((Color)value);
        }
    }


    //默认使用AES128_CBC_PKCS7加密，当然你也可以自己实现IEncryptor接口，定义自己的加密算法。
    byte[] iv = Encoding.ASCII.GetBytes("5CyM5tcL3yDFiWlN");
    byte[] key = Encoding.ASCII.GetBytes("W8fnmqMynlTJXPM1");

    IEncryptor encryptor = new DefaultEncryptor(key, iv);

    //序列化和反序列化类
    ISerializer serializer = new DefaultSerializer();

    //添加自定义的类型编码器
    serializer.AddTypeEncoder(new ColorTypeEncoder());

    //注册Preferences工厂
    BinaryFilePreferencesFactory factory = new BinaryFilePreferencesFactory(serializer, encryptor);
    Preferences.Register(factory);
```
更多的示例请查看教程 Basic Tutorials.unity

### 消息系统(Messenger)

Messenger用于应用模块间的通讯，它提供了消息订阅和发布的功能。Messenger支持按消息类型订阅和发布消息，也支持按channel来订阅和发布消息。
```csharp
    public class MessengerExample : MonoBehaviour
    {
        private IDisposable subscription;
        private IDisposable chatroomSubscription;
        private void Start()
        {
            //获得默认的Messenger
            Messenger messenger = Messenger.Default;

            //订阅一个消息,确保subscription是成员变量，否则subscription被GC回收时会自动退订消息
            subscription = messenger.Subscribe((PropertyChangedMessage<string> message) =>
            {
                Debug.LogFormat("Received Message:{0}", message);
            });

            //发布一个属性名改变的消息
            messenger.Publish(new PropertyChangedMessage<string>("clark", "tom", "Name"));

            //订阅聊天频道"chatroom1"的消息
            chatroomSubscription = messenger.Subscribe("chatroom1", (string message) =>
             {
                 Debug.LogFormat("Received Message:{0}", message);
             });

            //向聊天频道"chatroom1"发布一条消息
            messenger.Publish("chatroom1", "hello!");
        }

        private void OnDestroy()
        {
            if (this.subscription != null)
            {
                //退订消息
                this.subscription.Dispose();
                this.subscription = null;
            }

            if (this.chatroomSubscription != null)
            {
                //退订消息
                this.chatroomSubscription.Dispose();
                this.chatroomSubscription = null;
            }
        }
    }
```
更多的示例请查看教程 Basic Tutorials.unity

### 可观察的对象(Observables)

ObservableObject、ObservableList、ObservableDictionary，在MVVM框架的数据绑定中是必不可少的，它们分别实现了INotifyPropertyChanged和INotifyCollectionChanged接口，当对象的属性改变或者集合中Item变化时，我们能通过监听PropertyChanged和CollectionChanged事件可以收到属性改变和集合改变的通知，在数据绑定功能中，只有实现了这两个接口的对象在属性或者集合变化时，会自动通知UI视图改变，否则只能在初始绑定时给UI控件赋值一次，绑定之后改变视图模型的数值，无法通知UI控件修改。

下面我们看看ObservableDictionary的使用示例，当我们需要创建一个自定义的ListView控件时，我们需要了解其原理。
```csharp
    public class ObservableDictionaryExample : MonoBehaviour
    {
        private ObservableDictionary<int, Item> dict;

        protected void Start()
        {
    #if UNITY_IOS
            //在IOS中，泛型类型的字典，需要提供IEqualityComparer<TKey>，否则可能JIT异常
            this.dict = new ObservableDictionary<int, Item>(new IntEqualityComparer());
    #else
            this.dict = new ObservableDictionary<int, Item>();
    #endif
            dict.CollectionChanged += OnCollectionChanged;

            //添加Item
            dict.Add(1, new Item() { Title = "title1", IconPath = "xxx/xxx/icon1.png", Content = "this is a test." });
            dict.Add(2, new Item() { Title = "title2", IconPath = "xxx/xxx/icon2.png", Content = "this is a test." });

            //删除Item
            dict.Remove(1);

            //清除字典
            dict.Clear();
        }

        protected void OnDestroy()
        {
            if (this.dict != null)
            {
                this.dict.CollectionChanged -= OnCollectionChanged;
                this.dict = null;
            }
        }

        //集合改变事件
        protected void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            switch (eventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (KeyValuePair<int, Item> kv in eventArgs.NewItems)
                    {
                        Debug.LogFormat("ADD key:{0} item:{1}", kv.Key, kv.Value);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (KeyValuePair<int, Item> kv in eventArgs.OldItems)
                    {
                        Debug.LogFormat("REMOVE key:{0} item:{1}", kv.Key, kv.Value);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (KeyValuePair<int, Item> kv in eventArgs.OldItems)
                    {
                        Debug.LogFormat("REPLACE before key:{0} item:{1}", kv.Key, kv.Value);
                    }
                    foreach (KeyValuePair<int, Item> kv in eventArgs.NewItems)
                    {
                        Debug.LogFormat("REPLACE after key:{0} item:{1}", kv.Key, kv.Value);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Debug.LogFormat("RESET");
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
            }
        }
    }
```
更多的示例请查看教程 Basic Tutorials.unity

### 数据绑定(Databinding)

数据绑定是MVVM的关键技术，它用于将视图与视图模型进行绑定连接，视图和视图模型的连接可以是双向的，也可以是单向的，视图模型数据的改变可以通过数据绑定功能自动通知视图改变，同样视图的改变也可以通知视图模型数值进行改变。除了数值的连接外，数据绑定还可以支持事件、方法、命令的绑定。数据绑定在框架中是以一个服务模块的方式存在，它由很多的功能组件组成，如数据绑定上下文、类型转换器、表达式解析器、路径解析器、对象和方法代理、属性和Field的访问器等。数据绑定服务是可选的，只有在使用到框架的视图模块，且使用MVVM的方式来开发UI时，它是必要的。当然你也可以不使用本框架的视图模块，而仅仅使用数据绑定服务。

数据绑定服务是一个基础组件，我们可以在游戏初始化脚本中启动数据绑定服务，并且将所有的组件注册到全局上下文的服务容器中。如果有朋友想使用第三方的IoC组件，如Autofac、Zenject等，那么需要参考BindingServiceBundle的代码，将OnStart函数中初始化的所有类用其他的容器来创建。
```csharp
    //获得全局上下文
    ApplicationContext context = Context.GetApplicationContext();

    //初始化数据绑定服务
    BindingServiceBundle bindingService = new BindingServiceBundle(context.GetContainer());
    bindingService.Start();
```
如果安装了Lua插件，使用Lua编写游戏时，数据绑定服务初始化如下，LuaBindingServiceBundle中增加了有关对Lua对象支持的组件。
```csharp
    //获得全局上下文
    ApplicationContext context = Context.GetApplicationContext();

    //初始化数据绑定服务
    LuaBindingServiceBundle bundle = new LuaBindingServiceBundle(context.GetContainer());
    bundle.Start();
```
#### 数据绑定示例
```csharp
    //创建一个数据绑定集合，泛型参数DatabindingExample是视图，AccountViewModel是视图模型
    BindingSet<DatabindingExample, AccountViewModel> bindingSet;
    bindingSet = this.CreateBindingSet<DatabindingExample, AccountViewModel>();

    //绑定Text.text属性到Account.Username上，OneWay是单向,将Account.Username的值赋值到UI控件
    bindingSet.Bind(this.username).For(v => v.text).To(vm => vm.Account.Username).OneWay();

    //绑定InputField.text到Username属性，双向绑定，修改Username，自动更新InputField控件，修改InputField自动更新Username属性
    bindingSet.Bind(this.usernameEdit).For(v => v.text, v => v.onEndEdit).To(vm => vm.Username).TwoWay();

    //绑定Button到视图模型的OnSubmit方法，方向属性无效
    bindingSet.Bind(this.submit).For(v => v.onClick).To(vm => vm.OnSubmit);

    bindingSet.Build();
```
#### 绑定模式

- **OneWay**(View <-- ViewModel)

    单向绑定，只能视图模型修改视图中UI控件的值，ViewModel必须继承了INotifyPropertyChanged接口，并且属性值变化时会触发PropertyChanged事件，否则效果与OneTime一致，只有初始化绑定赋值一次。如Field则只能首次有效。

- **TwoWay**(View <--> ViewModel)

    双向绑定，视图控件修改，会自动修改视图模型，视图模型修改会自动修改视图控件。ViewModel必须支持PropertyChanged事件，UI控件必须支持onEndEdit事件，并且绑定了onEndEdit事件。

- **OneTime**(View <-- ViewModel)

    只赋值一次，只有在绑定关系初始化的时候将ViewModel的值赋值到视图控件上。

- **OneWayToSource**(View --> ViewModel)

    单向绑定，方向与OneWay相反，只能视图UI控件赋值到视图模型的属性。

#### 类型转换器(IConverter)

通常情况下，基本数据类型，当视图控件的字段类型与视图模型字段类型不一致时会自动转换，除非是无法自动转换的情况下才需要自定义类型转换器来支持。但是通过视图模型中保存的图片路径、图片名称或者图集精灵的名称，来修改视图控件上的图片或者图集精灵时，则必须通过类型转换器来转换。
```csharp
    //加载一个精灵图集
    Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
    foreach (var sprite in Resources.LoadAll<Sprite>("EquipTextures"))
    {
        if (sprite != null)
            sprites.Add(sprite.name, sprite);
    }

    //创建一个支持精灵名称到Sprite的转换器
    var spriteConverter = new SpriteConverter(sprites)；

    //获得转换器注册服务，它在数据绑定服务启动时会自动创建并注入上下文容器中
    IConverterRegistry converterRegistry = context.GetContainer().Resolve<IConverterRegistry>();

    //注册精灵转换器
    converterRegistry.Register("spriteConverter",spriteConverter);

    //通过视图模型Icon，修改精灵名称，通过spriteConverter转换为对应的Sprite，赋值到图片的sprite属性上。
    bindingSet.Bind(this.image).For(v => v.sprite).To(vm => vm.Icon).WithConversion("spriteConverter").OneWay();
```
请查看示例 ListView And Sprite Databinding Tutorials.unity

#### 绑定类型

- **属性和Field绑定**

    属性和Field绑定很简单，直接见示例
```csharp
      //C#，单向绑定
      bindingSet.Bind(this.username).For(v => v.text).To(vm => vm.Account.Username).OneWay();

      //C#，双向绑定，双向绑定时视图对象必须支持视图改变的事件，如“onEndEdit”，必须在For函数中配置
      bindingSet.Bind(this.usernameEdit).For(v => v.text, v => v.onEndEdit).To(vm => vm.Username).TwoWay();

      //C#，非拉姆达表达式的方式
      bindingSet.Bind (this.username).For ("text").To ("Account.Username").OneWay ();

      --Lua，非拉姆达表达式参数的版本
      bindingSet:Bind(self.username):For("text"):To("account.username"):OneWay()
      bindingSet:Bind(self.errorMessage):For("text"):To("errors['errorMessage']"):OneWay()
```
- **表达式绑定**

    表达式绑定只支持视图模型的一个或者多个属性，通过表达式转换为某个类型的值赋值到视图UI控件上，只能是OneTime或者OneWay的类型。表达式绑定函数，支持拉姆达表达式参数和string参数两种配置方式，C#代码只支持拉姆达表达式参数的方法，代码会自动分析表达式关注的视图模型的一个或者多个属性，自动监听这些属性的改变；Lua代码只支持使用string参数版本的方法，无法自动分析使用了视图模型的哪些属性，需要在参数中配置表达式所使用到的属性。
```csharp
      //C#代码，使用拉姆达表达式为参数的ToExpression方法，自动分析监听视图模型的Price属性
      bindingSet.Bind(this.price).For(v => v.text).ToExpression(vm => string.Format("${0:0.00}", vm.Price)).OneWay();

      --Lua代码，使用string参数版本的ToExpression方法，需要手动配置price属性,如果表达式使用了vm的多个属性，
      --则在"price"后继续配置其他属性
      bindingSet:Bind(self.price):For("text"):ToExpression(function(vm)
          return string.format(tostring("%0.2f"), vm.price)
      end ,"price"):OneWay()
```
- **方法绑定**

    方法绑定与属性绑定类似，也支持拉姆达表达式和字符串参数两个版本，方法绑定要确保控件的事件参数类型与视图模型被绑定方法的参数类型一致，否则可能导致绑定失败。
```csharp
      //C#，拉姆达表达式方式的绑定，Button.onClick 与视图模型的成员OnSubmit方法绑定
      bindingSet.Bind(this.submit).For(v => v.onClick).To(vm => vm.OnSubmit);

      //C#，拉姆达表达式方式的绑定，如果方法带参数，请在To后面加上泛型约束
      bindingSet.Bind(this.emailEdit).For(v => v.onValueChanged).To<string>(vm => vm.OnEmailValueChanged);

      --Lua，通过字符串参数绑定，Button.onClick 与视图模型的成员submit方法绑定
      bindingSet:Bind(self.submit):For("onClick"):To("submit"):OneWay()
```

- **命令和交互请求绑定**

    命令是对视图模型方法的一个包装，一般UI按钮onClick的绑定，既可以绑定到视图模型的一个方法，也可以绑定到视图模型的一个命令。但是建议绑定到命令上，命令不但可以响应按钮的点击事件，还能控制按钮的可点击状态，可以在按钮按下后立即使按钮置灰，在按钮事件响应完成后，重新恢复按钮状态。

    交互请求(InteractionRequest)交互请求往往都和命令配对使用，命令响应UI的点击事件，处理点击逻辑，交互请求向控制层发生消息控制UI的创建、修改和销毁。
```csharp
      //C#，绑定控制层的OnOpenAlert函数到交互请求AlertDialogRequest上
      bindingSet.Bind().For(v => this.OnOpenAlert).To(vm => vm.AlertDialogRequest);

      //绑定Button的onClick事件到OpenAlertDialog命令上
      bindingSet.Bind(this.openAlert).For(v => v.onClick).To(vm => vm.OpenAlertDialog);
```
- **集合的绑定**

    字典和列表的绑定跟属性/Field绑定基本差不多，见下面的代码
```csharp
      //C#，绑定一个Text.text属性到一个字典ObservableDictionary中key ="errorMessage" 对应的对象
      bindingSet.Bind(this.errorMessage).For(v => v.text).To(vm => vm.Errors["errorMessage"]).OneWay();
```
- **静态类绑定**

    静态类绑定和视图模型绑定唯一区别就是，静态类绑定创建的是静态绑定集，静态绑定集不需要视图模型对象。

      //C#，创建一个静态类的绑定集
      BindingSet<DatabindingExample> staticBindingSet = this.CreateBindingSet<DatabindingExample>();

      //绑定标题到类Res的一个静态变量databinding_tutorials_title
      staticBindingSet.Bind(this.title).For(v => v.text).To(() => Res.databinding_tutorials_title).OneWay();

- **本地化数据的绑定**

    本地化数据绑定请使用静态绑定集ToValue()函数绑定，首先通过Localization.GetValue()获得IObservableProperty对象，这是一个可观察的属性，切换语言时会收到值改变的通知，然后通过ToValue函数绑定，具体见下面的示例。
```csharp
      //C#，创建一个静态类型的绑定集
      BindingSet<DatabindingExample> staticBindingSet = this.CreateBindingSet<DatabindingExample>();

      var localization = Localization.Current;

      //通过本地化key获得一个IObservableProperty属性，
      //必须是IObservableProperty类型，否则切换语言不会更新
      var value = localization.GetValue("databinding.tutorials.title"); //OK        
      //var value = localization.Get<string>("databinding.tutorials.title"); //NO
      staticBindingSet.Bind(this.title).For(v => v.text).ToValue(value).OneWay();
```
#### Command Parameter

从事件到命令(ICommand)或方法的绑定支持自定义参数，使用Command Parameter可以为没有参数的UI事件添加一个自定义参数（如Button的Click事件），如果UI事件本身有参数则会被命令参数覆盖。使用Command Parameter可以很方便的将多个Button的Click事件绑定到视图模型的同一个函数OnClick(int buttonNo)上，请注意确保函数的参数类型和命令参数匹配，否则会导致错误。详情请参考下面的示例

在示例中将一组Button按钮的Click事件绑定到视图模型的OnClick函数上，通过参数buttonNo可以知道当前按下了哪个按钮。
```csharp
    public class ButtonGroupViewModel : ViewModelBase
    {
        private string text;
        private readonly SimpleCommand<int> click;
        public ButtonGroupViewModel()
        {
            this.click = new SimpleCommand<int>(OnClick);
        }

        public string Text
        {
            get { return this.text; }
            set { this.Set<string>(ref text, value, "Text"); }
        }

        public ICommand Click
        {
            get { return this.click; }
        }

        public void OnClick(int buttonNo)
        {
            Executors.RunOnCoroutineNoReturn(DoClick(buttonNo));
        }

        private IEnumerator DoClick(int buttonNo)
        {
            this.click.Enabled = false;
            this.Text = string.Format("Click Button:{0}.Restore button status after one second", buttonNo);
            Debug.LogFormat("Click Button:{0}", buttonNo);

            //Restore button status after one second
            yield return new WaitForSeconds(1f);
            this.click.Enabled = true;
        }

    }


    protected override void Start()
    {
        ButtonGroupViewModel viewModel = new ButtonGroupViewModel();

        IBindingContext bindingContext = this.BindingContext();
        bindingContext.DataContext = viewModel;

        /* databinding */
        BindingSet<DatabindingForButtonGroupExample, ButtonGroupViewModel> bindingSet;
        bindingSet = this.CreateBindingSet<DatabindingForButtonGroupExample, ButtonGroupViewModel>();
        bindingSet.Bind(this.button1).For(v => v.onClick).To(vm => vm.Click).CommandParameter(1);
        bindingSet.Bind(this.button2).For(v => v.onClick).To(vm => vm.Click).CommandParameter(2);
        bindingSet.Bind(this.button3).For(v => v.onClick).To(vm => vm.Click).CommandParameter(3);
        bindingSet.Bind(this.button4).For(v => v.onClick).To(vm => vm.Click).CommandParameter(4);
        bindingSet.Bind(this.button5).For(v => v.onClick).To(vm => vm.Click).CommandParameter(5);

        bindingSet.Bind(this.text).For(v => v.text).To(vm => vm.Text).OneWay();

        bindingSet.Build();
    }
```
#### Scope Key

在某些视图中，可能需要动态创建绑定关系，动态的移除绑定关系，这里我们提供了一种可以批量的移除绑定关系的方式，那就是Scope Key。
```csharp
    //C#,
    string scopeKey = "editKey";
    bindingSet.Bind(this.username).For(v => v.text).To(vm => vm.Account.Username).WithScopeKey(scopeKey).OneWay();
    bindingSet.Bind(this.submit).For(v => v.onClick).To(vm => vm.OnSubmit()).WithScopeKey(scopeKey);

    //通过Scope Key移除绑定
    this.ClearBindings(scopeKey); //or this.BindingContext().Clear(scopeKey)
```

#### 绑定的生命周期

一般来说数据绑定都在视图创建函数中来初始化，通过BindingSet来配置视图控件和视图模型之间的绑定关系，当调用BindingSet的Build函数时，Binder会创建BindingSet中所有的绑定关系对，被创建的绑定对会保存在当前视图的BindingContext中。BindingContext在首次调用时自动创建，同时自动生成了一个BindingContextLifecycle脚本，挂在当前视图对象上，由它来控制BindingContext的生命周期，当视图销毁时，BindingContext会随之销毁，存放在BindingContext中的绑定关系对也会随之销毁。

#### 注册属性和域的访问器

在IOS平台不允许JIT编译，不允许动态生成代码，数据绑定功能访问对象的属性、域和方法时无法像其他平台一样通过动态生成委托来访问，只能通过反射来访问，众所周知反射的效率是很差的，所以我提供了静态注入访问器的功能来绕过反射。默认情况下，我已经创建了UGUI和Unity引擎的部分类的属性访问器，参考我的代码，你也可以将视图模型类的常用属性的访问器注册到类型代理中。
```csharp
    public class UnityProxyRegister
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            Register<Transform, Vector3>("localPosition", t => t.localPosition, (t, v) => t.localPosition = v);
            Register<Transform, Vector3>("eulerAngles", t => t.eulerAngles, (t, v) => t.eulerAngles = v);
            Register<Transform, Vector3>("localEulerAngles", t => t.localEulerAngles, (t, v) => t.localEulerAngles = v);
            Register<Transform, Vector3>("right", t => t.right, (t, v) => t.right = v);
            Register<Transform, Vector3>("up", t => t.up, (t, v) => t.up = v);
            Register<Transform, Vector3>("forward", t => t.forward, (t, v) => t.forward = v);
            Register<Transform, Vector3>("position", t => t.position, (t, v) => t.position = v);
            Register<Transform, Vector3>("localScale", t => t.localScale, (t, v) => t.localScale = v);
            Register<Transform, Vector3>("lossyScale", t => t.lossyScale, null);
            Register<Transform, Quaternion>("rotation", t => t.rotation, (t, v) => t.rotation = v);
            Register<Transform, Quaternion>("localRotation", t => t.localRotation, (t, v) => t.localRotation = v);
            Register<Transform, Matrix4x4>("worldToLocalMatrix", t => t.worldToLocalMatrix, null);
            Register<Transform, Matrix4x4>("localToWorldMatrix", t => t.localToWorldMatrix, null);
            Register<Transform, int>("childCount", t => t.childCount, null);

            Register<RectTransform, Vector2>("offsetMax", t => t.offsetMax, (t, v) => t.offsetMax = v);
            Register<RectTransform, Vector2>("offsetMin", t => t.offsetMin, (t, v) => t.offsetMin = v);
            Register<RectTransform, Vector2>("pivot", t => t.pivot, (t, v) => t.pivot = v);
            Register<RectTransform, Vector2>("sizeDelta", t => t.sizeDelta, (t, v) => t.sizeDelta = v);
            Register<RectTransform, Vector2>("anchoredPosition", t => t.anchoredPosition, (t, v) => t.anchoredPosition = v);
            Register<RectTransform, Vector2>("anchorMax", t => t.anchorMax, (t, v) => t.anchorMax = v);
            Register<RectTransform, Vector3>("anchoredPosition3D", t => t.anchoredPosition3D, (t, v) => t.anchoredPosition3D = v);
            Register<RectTransform, Vector2>("anchorMin", t => t.anchorMin, (t, v) => t.anchorMin = v);
            Register<RectTransform, Rect>("rect", t => t.rect, null);

            Register<GameObject, bool>("activeSelf", t => t.activeSelf, (t, v) => t.SetActive(v));
            Register<GameObject, int>("layer", t => t.layer, (t, v) => t.layer = v);
            Register<GameObject, string>("tag", t => t.tag, (t, v) => t.tag = v);

            Register<Behaviour, bool>("enabled", t => t.enabled, (t, v) => t.enabled = v);
            Register<Behaviour, bool>("isActiveAndEnabled", t => t.isActiveAndEnabled, null);

            Register<Component, string>("tag", t => t.tag, (t, v) => t.tag = v);

            Register<Canvas, float>("planeDistance", t => t.planeDistance, (t, v) => t.planeDistance = v);
            Register<Canvas, string>("sortingLayerName", t => t.sortingLayerName, (t, v) => t.sortingLayerName = v);
            Register<Canvas, int>("sortingLayerID", t => t.sortingLayerID, (t, v) => t.sortingLayerID = v);
            Register<Canvas, int>("renderOrder", t => t.renderOrder, null);

            Register<CanvasGroup, float>("alpha", t => t.alpha, (t, v) => t.alpha = v);
            Register<CanvasGroup, bool>("interactable", t => t.interactable, (t, v) => t.interactable = v);
            Register<CanvasGroup, bool>("blocksRaycasts", t => t.blocksRaycasts, (t, v) => t.blocksRaycasts = v);
            Register<CanvasGroup, bool>("ignoreParentGroups", t => t.ignoreParentGroups, (t, v) => t.ignoreParentGroups = v);

            Register<GraphicRaycaster, bool>("ignoreReversedGraphics", t => t.ignoreReversedGraphics, (t, v) => t.ignoreReversedGraphics = v);

            Register<Mask, bool>("showMaskGraphic", t => t.showMaskGraphic, (t, v) => t.showMaskGraphic = v);

            Register<Selectable, SpriteState>("spriteState", t => t.spriteState, (t, v) => t.spriteState = v);
            Register<Selectable, ColorBlock>("colors", t => t.colors, (t, v) => t.colors = v);
            Register<Selectable, bool>("interactable", t => t.interactable, (t, v) => t.interactable = v);

            Register<Button, Button.ButtonClickedEvent>("onClick", t => t.onClick, null);

            Register<InputField, InputField.OnChangeEvent>("onValueChanged", t => t.onValueChanged, null);
            Register<InputField, InputField.SubmitEvent>("onEndEdit", t => t.onEndEdit, null);
            Register<InputField, string>("text", t => t.text, (t, v) => t.text = v);

            Register<Scrollbar, Scrollbar.ScrollEvent>("onValueChanged", t => t.onValueChanged, null);
            Register<Scrollbar, float>("size", t => t.size, (t, v) => t.size = v);
            Register<Scrollbar, float>("value", t => t.value, (t, v) => t.value = v);

            Register<Slider, Slider.SliderEvent>("onValueChanged", t => t.onValueChanged, null);
            Register<Slider, float>("value", t => t.value, (t, v) => t.value = v);
            Register<Slider, float>("maxValue", t => t.maxValue, (t, v) => t.maxValue = v);
            Register<Slider, float>("minValue", t => t.minValue, (t, v) => t.minValue = v);

            Register<Dropdown, int>("value", t => t.value, (t, v) => t.value = v);
            Register<Dropdown, Dropdown.DropdownEvent>("onValueChanged", t => t.onValueChanged, null);

            Register<Text, string>("text", t => t.text, (t, v) => t.text = v);
            Register<Text, int>("fontSize", t => t.fontSize, (t, v) => t.fontSize = v);

            Register<Toggle, bool>("isOn", t => t.isOn, (t, v) => t.isOn = v);
            Register<Toggle, Toggle.ToggleEvent>("onValueChanged", t => t.onValueChanged, (t, v) => t.onValueChanged = v);

            Register<ToggleGroup, bool>("allowSwitchOff", t => t.allowSwitchOff, (t, v) => t.allowSwitchOff = v);
        }

        static void Register<T, TValue>(string name, Func<T, TValue> getter, Action<T, TValue> setter)
        {
            var propertyInfo = typeof(T).GetProperty(name);
            if (propertyInfo is PropertyInfo)
            {
                ProxyFactory.Default.Register(new ProxyPropertyInfo<T, TValue>(name, getter, setter));
                return;
            }

            var fieldInfo = typeof(T).GetField(name);
            if (fieldInfo is FieldInfo)
            {
                ProxyFactory.Default.Register(new ProxyFieldInfo<T, TValue>(name, getter, setter));
                return;
            }

            throw new Exception(string.Format("Not found the property or field named '{0}' in {1} type", name, typeof(T).Name));
        }
    }
```
### UI框架

#### 动态变量集(Variables)

在UI的开发过程中，视图脚本往往需要访问、控制UI界面上的UI控件，通常来说，我们要么通过Transform.Find来查找，要么在View脚本中定义一个属性，在编辑UI界面时将控件拖放到这个属性上。第一种方式效率不高，第二种方式新增、删除都要重新改脚本属性，不是那么灵活。 在这里，我提供了第三中方式，VariableArray，这是一个动态的变量集，可以方便的新增和删除，又可以像一个成员属性一样使用。而且它不但支持所有的基本数据类型，还支持Unity组件类型、值类型。

![](images/Variable_UI.png)
```csharp
    //C#，访问变量
    Color color = this.variables.Get<Color>("color");
    InputField usernameInput = this.variables.Get<InputField>("username");
    InputField emailInput = this.variables.Get<InputField>("email");

    --Lua，可以直接通过self来访问变量，跟当前Lua表中的成员属性一样
    printf("vector:%s",self.vector:ToString())
    printf("color:%s",self.color:ToString())
    printf("username:%s",self.username.text)
    printf("email:%s",self.email.text)
```

#### UI视图定位器(IUIViewLocator)

UI视图定位器是一个查询和加载UI视图的服务，它提供了同步和异步加载UI视图的服务。根据项目的不同，可以自定义实现它的功能，你可以从Resources中加载视图，也可以从一个AssetBundle中加载视图，或者两者都支持。
```csharp
    //C#，创建一个默认的视图定位器，它支持从Resources中加载视图，如果要从AssetBundle中加载，需要自己实现
    IUIViewLocator locator = new DefaultUIViewLocator()

    //通过UI视图定位器，根据一个UI路径名加载一个Loading的窗口视图
    var window = locator.LoadWindow<LoadingWindow>("UI/Loading");
    window.Show();
```
#### UI视图动画(Animations)

根据一个UI视图打开、关闭、获得焦点、失去焦点的过程，视图动画可以分为入场动画、出场动画、激活动画、钝化动画。继承UIAnimation或者IAnimation，使用DoTween、iTween等，可以创建自己满意的UI动画。

在框架中UIView支持入场动画和出场动画，当打开一个视图或者隐藏一个视图时会可以播放动画。而Window除了支持入场动画和出场动画，还支持激活动画和钝化动画，并且自动控制播放，当一个Window获得焦点时播放激活动画，当失去焦点是播放钝化动画。

如下所示，在Examples中，我创建了一个渐隐渐显的动画，将他们挂在一个Window视图上，并设置为入场动画和出场动画，当窗口打开时逐渐显现，当窗口关闭时慢慢消失。

自定义一个C#的渐隐渐显动画

![](images/Animations_Alpha.png)
```csharp
    public class AlphaAnimation : UIAnimation
    {
        [Range (0f, 1f)]
        public float from = 1f;
        [Range (0f, 1f)]
        public float to = 1f;

        public float duration = 2f;

        private IUIView view;

        void OnEnable ()
        {
            this.view = this.GetComponent<IUIView> ();
            switch (this.AnimationType) {
            case AnimationType.EnterAnimation:
                this.view.EnterAnimation = this;
                break;
            case AnimationType.ExitAnimation:
                this.view.ExitAnimation = this;
                break;
            case AnimationType.ActivationAnimation:
                if (this.view is IWindowView)
                    (this.view as IWindowView).ActivationAnimation = this;
                break;
            case AnimationType.PassivationAnimation:
                if (this.view is IWindowView)
                    (this.view as IWindowView).PassivationAnimation = this;
                break;
            }

            if (this.AnimationType == AnimationType.ActivationAnimation
                    || this.AnimationType == AnimationType.EnterAnimation)
            {
                this.view.CanvasGroup.alpha = from;
            }
        }

        public override IAnimation Play ()
        {
            this.view.CanvasGroup.DOFade (this.to, this.duration)
            .OnStart (this.OnStart)
            .OnComplete (this.OnEnd)
            .Play ();
            return this;
        }
    }
```
使用DoTween自定义一个Lua的动画

![](images/Animations_Alpha_Lua.png)

    require("framework.System")

    ---
    --模块
    --@module AlphaAnimation
    local M=class("AlphaAnimation",target)

    function M:play(view,startCallback,endCallback)
        view.CanvasGroup:DOFade(self.to, self.duration)
            :OnStart(function() startCallback() end)
            :OnComplete(function() endCallback() end)
            :Play()    
    end

    return M

#### UI控件

UGUI虽然为我们提供了丰富的UI控件库，但是在某些时候，仍然无法满足我们的要求，比如我们需要一个性能优越的ListView，这时候我们就需要自定义自己的UI控件。在本框架中，我提供了一些常用的UI控件，比如AlertDialog、Loading、Toast等，在Examples/Resources/UI目录下，你能找到默认的视图界面，参考这些界面可以重新定义界面外观，修改静态类的ViewName属性可以重新制定视图的加载路径。

下面以AlertDialog为例来介绍它们的用法

![](images/AlertDialog.png)
```csharp
    //对话框视图默认目录路径是UI/AlertDialog，可以通过如下方式修改视图路径
    AlertDialog.ViewName = "Your view directory/AlertDialog";

    //C#，打开一个对话框窗口
    AlertDialog.ShowMessage("This is a dialog test.", "Interation Example", "Yes", null, "No", true,
    result =>
    {
        Debug.LogFormat("Result:{0}",result);
    });
```
#### 视图、窗口和窗口管理器

- **视图(IView/IUIView)**

    视图通俗的讲就是展现给用户所看到的UI界面、图像、动画等。在本框架中，根据游戏视图层的特点，将其分成两大类，场景视图和UI视图。UI视图对应的是IUIView接口，而场景视图对应的是IView接口。    

- **视图组(IViewGroup/IUIViewGroup)**

    视图组是一个视图的集合，也可以说是视图容器，它有多个视图组成，在视图组中可以添加、删除子视图。同时视图组本身也是一个视图，它同样可以做为其他视图组的子视图。

    在UI开发中，我们经常会发现一个UI界面可以划分很多的区域，比如Top栏，左边栏，右边栏，Bottom栏，内容区域等等，并且有些部分在多个UI界面之间是可以共享使用的。根据这些特点，我就可以将不同的区域分别做成不同的视图，在最后界面显示时，通过视图组装配成完整的视图，这样既有助于提高代码的重复利用，又大大降低了代码的耦合性和复杂性。**重点说一下，我们可以用这种设计思路来设计游戏的新手引导系统，只有界面需要显示引导时，才将引导界面动态插入到当前的界面中。新手引导的逻辑与正常游戏逻辑完全分离，避免造成引导逻辑和游戏逻辑的高度耦合。**

    同样，在游戏场景视图中，我们也可以将复杂视图拆分成大大小小的视图组和子视图，并且在游戏过程中，动态的添加和删除子视图。比如一个游戏角色，就是场景中的一个子视图，当角色进入视野时添加视图，当从视野消失时，删除视图。

    以王者荣耀日常活动界面为例，可以拆分为顶菜单栏、左侧菜单栏和内容区域，菜单栏视图可以复用，每次只需要改变内容区域的视图即可。

    ![](images/View_Example.png)

- **窗口(IWindow)**

    Window是一个UI界面视图的根容器(IUIViewGroup、IUIView)，同时也是一个控制器，它负责创建、销毁、显示、隐藏窗口视图，负责管理视图、视图模型的生命周期，负责创建子窗口、与子窗口交互等。
```csharp
      //C#，创建窗口
      public class ExampleWindow : Window
      {
          public Text progressBarText;
          public Slider progressBarSlider;
          public Text tipText;
          public Button button;

          protected override void OnCreate(IBundle bundle)
          {
              BindingSet<ExampleWindow, ExampleViewModel> bindingSet;
              bindingSet = this.CreateBindingSet(new ExampleViewModel());

              bindingSet.Bind(this.progressBarSlider).For("value", "onValueChanged").To("ProgressBar.Progress").TwoWay();
              bindingSet.Bind(this.progressBarSlider.gameObject).For(v => v.activeSelf)
              .To(vm => vm.ProgressBar.Enable).OneWay();
              bindingSet.Bind(this.progressBarText).For(v => v.text)
              .ToExpression(
                  vm => string.Format("{0}%", Mathf.FloorToInt(vm.ProgressBar.Progress * 100f)))
              .OneWay();
              bindingSet.Bind(this.tipText).For(v => v.text).To(vm => vm.ProgressBar.Tip).OneWay();
              bindingSet.Bind(this.button).For(v => v.onClick).To(vm => vm.Click).OneWay();
              binding,bound to the onClick event and interactable property.
              bindingSet.Build();
          }

          protected override void OnDismiss()
          {
          }
      }
```
      --Lua,创建窗口
      require("framework.System")

      local ExampleViewModel = require("LuaUI.Startup.ExampleViewModel")

      ---
      --模块
      --@module ExampleWindow
      local M=class("ExampleWindow",target)

      function M:onCreate(bundle)
          self.viewModel = ExampleViewModel()

          self:BindingContext().DataContext = self.viewModel

          local bindingSet = self:CreateBindingSet()

          bindingSet:Bind(self.progressBarSlider):For("value", "onValueChanged"):To("progressBar.progress"):TwoWay()
          bindingSet:Bind(self.progressBarSlider.gameObject):For("activeSelf"):To("progressBar.enable"):OneWay()
          bindingSet:Bind(self.progressBarText):For("text"):ToExpression(
              function(vm) return string.format("%0.2f%%",vm.progressBar.progress * 100) end,
          "progressBar.progress"):OneWay()
          bindingSet:Bind(self.tipText):For("text"):To("progressBar.tip"):OneWay()
          bindingSet:Bind(self.button):For("onClick"):To("command"):OneWay()
          bindingSet:Build()
      end

      return M

- **窗口容器和窗口管理器(WindowContainer、IWindowManager)**

    窗口管理器是一个管理窗口的容器，游戏启动时首先需要创建一个全局的窗口管理器GlobalWindowManager，将它挂在最外层的根Canvas上（见下图），在这个根Canvas下创建编辑其他的窗口视图。

    ![](images/WindowManager.png)

    窗口容器既是一个窗口管理器，又是一个窗口，在窗口容器中可以添加、删除子窗口、管理子窗口，也可以像一个普通窗口一样显示、隐藏。拿我们的MMO游戏来说，一般会创建一个名为"Main"的主窗口容器和一个"Battle"的窗口容器，在主界面打开的所有窗口视图都会放入到Main容器中，但是当进入某个战斗副本时，会将Main容器隐藏，将"Battle"容器显示出来，战斗副本中所有UI窗口都会用Battle容器来管理，退出副本时，只需要关闭Battle容器，设置Main容器可见，就可以轻松恢复Main容器中窗口的层级关系。
```csharp
      //C#，创建一个MAIN容器，默认会在全局窗口管理器中创建
      WindowContainer winContainer = WindowContainer.Create("MAIN");
      IUIViewLocator locator = context.GetService<IUIViewLocator>();

      //在MAIN容器中打开一个窗口
      StartupWindow window = locator.LoadWindow<StartupWindow>(winContainer, "UI/Startup/Startup");
      ITransition transition = window.Show()    
```
- **窗口类型**

  窗口类型按不同的功能特征分为FULL、POPUP、QUEUED_POPUP、DIALOG、PROGRESS五种类型。

    - 全屏窗口(FULL)

        全屏窗口一般为全屏显示，它优先级较低。

    - 弹出窗口(POPUP)

        弹出窗口在被其他窗口覆盖时，会自动关闭，但是可以通过ITransition.Overlay()函数重写覆盖规则；
```csharp
            var window = ...
            window.Show().Overlay((previous,current) =>
            {
                 if (previous == null || previous.WindowType == WindowType.FULL)
                    return ActionType.None;

                if (previous.WindowType == WindowType.POPUP)
                    return ActionType.Dismiss;

                return ActionType.None;
            });
```
       以上代码覆盖默认的规则，通过它可以控制前一个窗口的关闭和隐藏等。

    - 系统对话窗(DIALOG)

        系统对话窗和进度窗口有最高的优先级，在同一个窗口管理器中，它会显示在最顶层，并且只允许打开一个，当有对话窗或者进度窗口显示时，如果打开其他窗口，其他窗口不会显示，只有当系统对话框或者进度窗关闭时其它窗口才会显示出来，如果同时打开多个对话窗，对话窗口会排队处理，只有关闭前一个才会显示下一个；系统对话窗一般用来处理网络断开提示重连，或者退出游戏时提示用户确认等。

    - 进度条对话窗(PROGRESS)

        功能等同于系统对话窗，在显示进度条对话窗时使用。

    - 队列弹窗(QUEUED_POPUP)

        队列弹窗(QUEUED_POPUP)功能类似DIALOG类型，但是可以配置窗口优先级，在同一个窗口管理器，它只允许打开一个，当有其他的QUEUED_POPUP或者POPUP、FULL等窗口打开时，会排队等候，并且优先级高的QUEUED_POPUP窗口先打开，优先级低的后打开，其他窗口最后打开，队列弹窗(QUEUED_POPUP)优先级低于DIALOG和PROGRESS窗口，如果有DIALOG或者PROGRESS窗口打开时会被覆盖。

        这种类型的窗口一般用来展示服务器推送的消息上，比如游戏启动时，服务器推送多个消息，打开公告牌，领取奖励等等，需要打开多个窗口显示时可以使用这种类型，并且对弹出窗口排序。

    窗口类型设置如下图：

  ![](images/WindowType.png)  

#### 交互请求(InteractionRequest)

交互请求(InteractionRequest)在MVVM框架的使用中，我认为是最难理解，最复杂和最绕的地方，而且在网上很多的MVVM示例中，也没有讲到这部分，为什么我们需要交互请求呢？交互请求解决了什么问题？引入交互请求主要目的是为了视图模型(ViewModel)和视图(View)解耦，**在视图模型中，我们不应该创建、引用和直接控制视图，因为那是控制层的工作，不应该是视图模型层的工作，视图层可以依赖视图模型层，但是反之则不允许，切记**。在一个按钮(Button)的点击事件中，往往会触发视图的创建或者销毁，而在MVVM中，按钮点击事件一般都会绑定到视图模型层的一个命令（ICommand）上，即绑定到视图模型的一个成员方法上，在这个方法中往往除了视图无关的逻辑外，还包含了控制视图的创建、打开、销毁的逻辑，前文中提到，这些逻辑会造成对视图层引用和依赖，这是不允许的，所以我们就引入了交互请求(InteractionRequest)的概念，通过交互请求，将视图控制的逻辑发回到控制层中处理（在本框架中就是View、Window脚本，它们既是视图层又是控制层，见前面章节中MVVM架构图）。

请看下面的代码示例，使用交互请求来打开一个警告对话窗，同时在对话窗关闭时，收到用户选择的结果。
```csharp
    public class InteractionExampleViewModel : ViewModelBase
    {
        private InteractionRequest<DialogNotification> alertDialogRequest;

        private SimpleCommand openAlertDialog;

        public InteractionExampleViewModel()
        {
            //创建一个交互请求，这个交互请求的作用就是向控制层(InteractionExample)发送一个打开对话窗的通知
            this.alertDialogRequest = new InteractionRequest<DialogNotification>(this);

            //创建一个打响应按钮事件的命令
            this.openAlertDialog = new SimpleCommand(Click);
        }

        public IInteractionRequest AlertDialogRequest { get { return this.alertDialogRequest; } }

        public ICommand OpenAlertDialog { get { return this.openAlertDialog; } }

        public void Click()
        {
            //设置命令的Enable为false，通过数据绑定解耦，间接将视图层按钮设置为不可点击状态
            this.openAlertDialog.Enabled = false;

            //创建一个对话框通知
            DialogNotification notification = new DialogNotification("Interation Example",
                "This is a dialog test.", "Yes", "No", true);

            //创建一个回调函数，此回调函数会在AlertDialog对话框关闭时调用
            Action<DialogNotification> callback = n =>
            {
                //设置命令的Enable为true，通过绑定会自动恢复按钮的点击状态
                this.openAlertDialog.Enabled = true;

                if (n.DialogResult == AlertDialog.BUTTON_POSITIVE)
                {
                    //对话框Yes按钮被按下
                    Debug.LogFormat("Click: Yes");
                }
                else if (n.DialogResult == AlertDialog.BUTTON_NEGATIVE)
                {
                    //对话框No按钮被按下
                    Debug.LogFormat("Click: No");
                }
            };

            //交互请求向View层OnOpenAlert函数发送通知
            this.alertDialogRequest.Raise(notification, callback);
        }
    }

    public class InteractionExample : WindowView
    {
        public Button openAlert;
        protected override void Start()
        {
            InteractionExampleViewModel viewModel = new InteractionExampleViewModel();
            this.SetDataContext(viewModel);

            //创建一个bindingSet
            BindingSet<InteractionExample, InteractionExampleViewModel> bindingSet;
            bindingSet = this.CreateBindingSet<InteractionExample, InteractionExampleViewModel>();

            //绑定本视图的OnOpenAlert函数到视图模型的交互请求AlertDialogRequest，当交互请求触发时，自动调用OnOpenAlert函数
            bindingSet.Bind().For(v => this.OnOpenAlert(null, null)).To(vm => vm.AlertDialogRequest);

            //绑定按钮的onClick事件到视图模型的OpenAlertDialog命令上
            bindingSet.Bind(this.openAlert).For(v => v.onClick).To(vm => vm.OpenAlertDialog);

            bindingSet.Build();
        }

        //创建和打开对话框的函数，通过交互请求触发
        private void OnOpenAlert(object sender, InteractionEventArgs args)
        {
            //收到视图模型层交互请求alertDialogRequest发来的通知

            //得到通知数据
            DialogNotification notification = args.Context as DialogNotification;

            //得到AlertDialog窗口关闭时的回调函数
            var callback = args.Callback;

            if (notification == null)
                return;

            //创建一个对话窗
            AlertDialog.ShowMessage(notification.Message, notification.Title, notification.ConfirmButtonText,
                null,
                notification.CancelButtonText,
                notification.CanceledOnTouchOutside,
                (result) =>
                {
                    //将对话窗按钮事件响应结果赋值到notification，传递到视图模型层使用
                    notification.DialogResult = result;

                    //对话窗关闭时，调用交互请求中设置的回调函数，通知视图模型层处理后续逻辑
                    if (callback != null)
                        callback();
                });
        }
    }
```
请查看示例 Interaction Tutorials.unity

#### 交互行为(InteractionAction)

InteractionAction配合InteractionRequest配对使用，由交互请求发起交互申请，由交互行为来完成交互的任务，它是对上一节中视图方法绑定到交互请求的一个扩展，通常来说使用方法绑定交互请求就可以了，但是针对一些通用的功能，比如请求开启或者关闭一个Loading窗可以用InteractionAction来实现，以方便代码重用，在不同的视图中，只需要创建一个LoadingInteractionAction实例就可以完成Loading窗的开启功能。下面请看开启Loading的示例
```csharp
    //在ViewModel中创建一个交互请求
    this.loadingRequest = new InteractionRequest<VisibilityNotification>();

    //在ViewModel中创建一个显示Loading窗口的命令，通过命令调用交互请求打开一个Loading界面
    this.ShowLoading = new SimpleCommand(() =>
    {
        VisibilityNotification notification = new VisibilityNotification(true);
        this.loadingRequest.Raise(notification);
    });


    //在View中创建一个交互请求LoadingInteractionAction
    this.loadingInteractionAction = new LoadingInteractionAction();

    //绑定InteractionAction到InteractionRequest
    bindingSet.Bind().For(v => v.loadingInteractionAction).To(vm => vm.LoadingRequest);
```
请查看示例 Interaction Tutorials.unity

#### 集合与列表视图的绑定
在Unity3D游戏开发中，我们经常要使用到UGUI的ScrollRect控件，比如我们要展示一个装备列表，或者一个背包中的所有物品。那么我们可以使用数据绑定功能来自动更新列表中的内容吗，比如添加、删除、修改一个装备集合中的数据，装备列表视图会自动更新界面内容吗？ 答案是肯定的，使用ObservableList或者ObservableDictionary集合来存储装备信息，通过数据绑定集合到一个视图脚本上，就可以自动的更新装备列表的内容，只是这里的视图脚本需要我们自己实现，因为每个项目列表视图并不是标准化的，我无法提供一个通用的脚本来提供集合的绑定。

下面的示例中我创建了一个ListView的视图脚本，使用它来动态更新一个装备列表的视图。

![](images/Tutorials_ListView.png)


首先我们创建一个ListView控件，通过这个控件来监听装备集合ObservableDictionary的改变，当集合中内容变化时，自动更新UGUI视图，向装备列表中添加、删除装备。
```csharp
    public class ListView : UIView
    {
        public class ItemClickedEvent : UnityEvent<int>
        {
            public ItemClickedEvent()
            {
            }
        }

        private ObservableList<ListItemViewModel> items;

        public Transform content;

        public GameObject itemTemplate;

        public ItemClickedEvent OnSelectChanged = new ItemClickedEvent();

        //装备集合，通过数据绑定赋值
        public ObservableList<ListItemViewModel> Items
        {
            get { return this.items; }
            set
            {
                if (this.items == value)
                    return;

                if (this.items != null)
                    this.items.CollectionChanged -= OnCollectionChanged;

                this.items = value;

                this.OnItemsChanged();

                if (this.items != null)
                    this.items.CollectionChanged += OnCollectionChanged;
            }
        }

        /// <summary>
        /// 监听装备集合的改变，自动更新装备列表界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        protected void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            switch (eventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    this.AddItem(eventArgs.NewStartingIndex, eventArgs.NewItems[0]);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    this.RemoveItem(eventArgs.OldStartingIndex, eventArgs.OldItems[0]);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    this.ReplaceItem(eventArgs.OldStartingIndex, eventArgs.OldItems[0], eventArgs.NewItems[0]);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    this.ResetItem();
                    break;
                case NotifyCollectionChangedAction.Move:
                    this.MoveItem(eventArgs.OldStartingIndex, eventArgs.NewStartingIndex, eventArgs.NewItems[0]);
                    break;
            }
        }

        protected virtual void OnItemsChanged()
        {
            for (int i = 0; i < this.items.Count; i++)
            {
                this.AddItem(i, items[i]);
            }
        }

        protected virtual void OnSelectChange(GameObject itemViewGo)
        {
            if (this.OnSelectChanged == null || itemViewGo == null)
                return;

            for (int i = 0; i < this.content.childCount; i++)
            {
                var child = this.content.GetChild(i);
                if (itemViewGo.transform == child)
                {
                    this.OnSelectChanged.Invoke(i);
                    break;
                }
            }
        }

        protected virtual void AddItem(int index, object item)
        {
            var itemViewGo = Instantiate(this.itemTemplate);
            itemViewGo.transform.SetParent(this.content, false);
            itemViewGo.transform.SetSiblingIndex(index);

            Button button = itemViewGo.GetComponent<Button>();
            button.onClick.AddListener(() => OnSelectChange(itemViewGo));
            itemViewGo.SetActive(true);

            UIView itemView = itemViewGo.GetComponent<UIView>();
            itemView.SetDataContext(item);
        }

        protected virtual void RemoveItem(int index, object item)
        {
            Transform transform = this.content.GetChild(index);
            UIView itemView = transform.GetComponent<UIView>();
            if (itemView.GetDataContext() == item)
            {
                itemView.gameObject.SetActive(false);
                Destroy(itemView.gameObject);
            }
        }

        protected virtual void ReplaceItem(int index, object oldItem, object item)
        {
            Transform transform = this.content.GetChild(index);
            UIView itemView = transform.GetComponent<UIView>();
            if (itemView.GetDataContext() == oldItem)
            {
                itemView.SetDataContext(item);
            }
        }

        protected virtual void MoveItem(int oldIndex, int index, object item)
        {
            Transform transform = this.content.GetChild(oldIndex);
            UIView itemView = transform.GetComponent<UIView>();
            itemView.transform.SetSiblingIndex(index);
        }

        protected virtual void ResetItem()
        {
            for (int i = this.content.childCount - 1; i >= 0; i--)
            {
                Transform transform = this.content.GetChild(i);
                Destroy(transform.gameObject);
            }
        }
    }
```
然后创建一个装备列表的Item视图ListItemView，它负责将Item视图上的UGUI控件和装备的视图模型绑定，当装备的视图模型改变时，自动更新Item视图的内容。
```csharp
    public class ListItemView : UIView
    {
        public Text title;
        public Text price;
        public Image image;
        public GameObject border;

        protected override void Start()
        {
            //绑定Item上的视图元素
            BindingSet<ListItemView, ListItemViewModel> bindingSet = this.CreateBindingSet<ListItemView, ListItemViewModel>();
            bindingSet.Bind(this.title).For(v => v.text).To(vm => vm.Title).OneWay();
            bindingSet.Bind(this.image).For(v => v.sprite).To(vm => vm.Icon).WithConversion("spriteConverter").OneWay();
            bindingSet.Bind(this.price).For(v => v.text).ToExpression(vm => string.Format("${0:0.00}", vm.Price)).OneWay();
            bindingSet.Bind(this.border).For(v => v.activeSelf).To(vm => vm.IsSelected).OneWay();
            bindingSet.Build();
        }
    }
```

最后是ListView控件和ListItemView的视图模型代码如下。
```csharp
    public class ListViewViewModel : ViewModelBase
    {
        private readonly ObservableList<ListItemViewModel> items = new ObservableList<ListItemViewModel>();

        public ObservableList<ListItemViewModel> Items
        {
            get { return this.items; }
        }

        public ListItemViewModel SelectedItem
        {
            get
            {
                foreach (var item in items)
                {
                    if (item.IsSelected)
                        return item;
                }
                return null;
            }
        }

        public void AddItem()
        {
            int i = this.items.Count;
            int iconIndex = Random.Range(1, 30);
            this.items.Add(new ListItemViewModel() {
                Title = "Equip " + i,
                Icon = string.Format("EquipImages_{0}", iconIndex),
                Price = Random.Range(10f, 100f)
            });
        }

        public void RemoveItem()
        {
            if (this.items.Count <= 0)
                return;

            int index = Random.Range(0, this.items.Count - 1);
            this.items.RemoveAt(index);
        }

        public void ClearItem()
        {
            if (this.items.Count <= 0)
                return;

            this.items.Clear();
        }

        public void ChangeItemIcon()
        {
            if (this.items.Count <= 0)
                return;

            foreach (var item in this.items)
            {
                int iconIndex = Random.Range(1, 30);
                item.Icon = string.Format("EquipImages_{0}", iconIndex);
            }
        }

        public void Select(int index)
        {
            if (index <= -1 || index > this.items.Count - 1)
                return;

            for (int i = 0; i < this.items.Count; i++)
            {
                if (i == index)
                {
                    items[i].IsSelected = !items[i].IsSelected;
                    if (items[i].IsSelected)
                        Debug.LogFormat("Select, Current Index:{0}", index);
                    else
                        Debug.LogFormat("Cancel");
                }
                else
                {
                    items[i].IsSelected = false;
                }
            }
        }
    }

    public class ListItemViewModel : ViewModelBase
    {
        private string title;
        private string icon;
        private float price;
        private bool selected;

        public string Title
        {
            get { return this.title; }
            set { this.Set<string>(ref title, value, "Title"); }
        }
        public string Icon
        {
            get { return this.icon; }
            set { this.Set<string>(ref icon, value, "Icon"); }
        }

        public float Price
        {
            get { return this.price; }
            set { this.Set<float>(ref price, value, "Price"); }
        }

        public bool IsSelected
        {
            get { return this.selected; }
            set { this.Set<bool>(ref selected, value, "IsSelected"); }
        }
    }

    public class ListViewDatabindingExample : MonoBehaviour
    {
        private int itemCount;
        private ListViewViewModel viewModel;

        public Button addButton;

        public Button removeButton;

        public Button clearButton;

        public Button changeIconButton;

        public ListView listView;

        void Awake()
        {
            ApplicationContext context = Context.GetApplicationContext();
            BindingServiceBundle bindingService = new BindingServiceBundle(context.GetContainer());
            bindingService.Start();

            Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
            foreach (var sprite in Resources.LoadAll<Sprite>("EquipTextures"))
            {
                if (sprite != null)
                    sprites.Add(sprite.name, sprite);
            }
            IConverterRegistry converterRegistry = context.GetContainer().Resolve<IConverterRegistry>();
            converterRegistry.Register("spriteConverter", new SpriteConverter(sprites));
        }

        void Start()
        {
            viewModel = new ListViewViewModel();
            for (int i = 0; i < 3; i++)
            {
                viewModel.AddItem();
            }

            IBindingContext bindingContext = this.BindingContext();
            bindingContext.DataContext = viewModel;

            BindingSet<ListViewDatabindingExample, ListViewViewModel> bindingSet;
            bindingSet = this.CreateBindingSet<ListViewDatabindingExample, ListViewViewModel>();
            bindingSet.Bind(this.listView).For(v => v.Items).To(vm => vm.Items).OneWay();
            bindingSet.Bind(this.listView).For(v => v.OnSelectChanged).To(vm => vm.Select(0)).OneWay();

            bindingSet.Bind(this.addButton).For(v => v.onClick).To(vm => vm.AddItem());
            bindingSet.Bind(this.removeButton).For(v => v.onClick).To(vm => vm.RemoveItem());
            bindingSet.Bind(this.clearButton).For(v => v.onClick).To(vm => vm.ClearItem());
            bindingSet.Bind(this.changeIconButton).For(v => v.onClick).To(vm => vm.ChangeItemIcon());

            bindingSet.Build();
        }
    }
```
请查看示例 ListView And Sprite Databinding Tutorials.unity

#### 数据绑定与异步加载精灵
在前文的示例中，我有使用到精灵的绑定，只是它是提前加载到内存中的。在这里我将讲讲如何通过数据绑定来异步加载一个精灵。与上一节中集合绑定类似，通过一个视图脚本就可以轻松实现精灵的异步加载。下面我们来看示例。

点击图中的"Change Icon"按钮改变图标，图标的加载为异步加载的方式，有一个加载动画。

![](images/Tutorials_SpriteUI.png)

首先，我们实现一个精灵异步加载器，将它挂在需要异步加载精灵图片的Image控件上。

![](images/Tutorials_Sprite.png)

```csharp
    [RequireComponent(typeof(Image))]
    public class AsyncSpriteLoader : MonoBehaviour
    {
        private Image target;
        private string spriteName;
        public Sprite defaultSprite;
        public Material defaultMaterial;
        public string spritePath;

        public string SpriteName
        {
            get { return this.spriteName; }
            set
            {
                if (this.spriteName == value)
                    return;

                this.spriteName = value;
                if (this.target != null)
                    this.OnSpriteChanged();
            }
        }

        protected virtual void OnEnable()
        {
            this.target = this.GetComponent<Image>();
        }

        protected virtual void OnSpriteChanged()
        {
            if (string.IsNullOrEmpty(this.spriteName))
            {
                this.target.sprite = null;
                this.target.material = null;
                return;
            }

            this.target.sprite = defaultSprite;
            this.target.material = defaultMaterial;

            StartCoroutine(LoadSprite());
        }

        /// <summary>
        /// 异步加载精灵，为了效果明显，在加载器等待了一秒钟
        /// </summary>
        /// <returns></returns>
        IEnumerator LoadSprite()
        {
            yield return new WaitForSeconds(1f);

            Sprite[] sprites = Resources.LoadAll<Sprite>(this.spritePath);
            foreach(var sprite in sprites)
            {
                if(sprite.name.Equals(this.spriteName))
                {
                    this.target.sprite = sprite;
                    this.target.material = null;
                }
            }
        }
    }
```
然后创建示例界面的视图和视图模型代码如下
```csharp
    public class SpriteViewModel : ViewModelBase
    {
        private string spriteName = "EquipImages_1";

        public string SpriteName
        {
            get { return this.spriteName; }
            set { this.Set<string>(ref spriteName, value, "SpriteName"); }
        }

        public void ChangeSpriteName()
        {
            this.SpriteName = string.Format("EquipImages_{0}", Random.Range(1, 30));
        }
    }

    public class DatabindingForAsyncLoadingSpriteExample : MonoBehaviour
    {
        public Button changeSpriteButton;

        public AsyncSpriteLoader spriteLoader;

        void Awake()
        {
            ApplicationContext context = Context.GetApplicationContext();
            BindingServiceBundle bindingService = new BindingServiceBundle(context.GetContainer());
            bindingService.Start();
        }

        void Start()
        {
            var viewModel = new SpriteViewModel();

            IBindingContext bindingContext = this.BindingContext();
            bindingContext.DataContext = viewModel;

            BindingSet<DatabindingForAsyncLoadingSpriteExample, SpriteViewModel> bindingSet;
            bindingSet = this.CreateBindingSet<DatabindingForAsyncLoadingSpriteExample, SpriteViewModel>();
            bindingSet.Bind(this.spriteLoader).For(v => v.SpriteName).To(vm => vm.SpriteName).OneWay();

            bindingSet.Bind(this.changeSpriteButton).For(v => v.onClick).To(vm => vm.ChangeSpriteName());

            bindingSet.Build();
        }
    }
```
请查看示例 Databinding for Asynchronous Loading Sprites Tutorials.unity

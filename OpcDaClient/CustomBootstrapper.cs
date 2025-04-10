using Nancy;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;

namespace OpcDaSubscription
{
    public class CustomBootstrapper : DefaultNancyBootstrapper
    {
        protected override IRootPathProvider RootPathProvider => new Nancy.Hosting.Self.FileSystemRootPathProvider();

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);
            // 可以在此添加其他启动配置
        }
    }
}

namespace OpcDaClient
{
    public class OpcDaClientBootstrapper : DefaultNancyBootstrapper
    {
        protected override void ConfigureApplicationContainer(TinyIocContainer container)
        {
            base.ConfigureApplicationContainer(container);
            container.Register<IMetadataModule, MetadataModule>();
        }
    }
}
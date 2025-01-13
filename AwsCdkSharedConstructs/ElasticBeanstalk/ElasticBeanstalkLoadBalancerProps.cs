namespace AwsCdkSharedConstructs.ElasticBeanstalk;

public class ElasticBeanstalkLoadBalancerProps 
{
    public LoadBalancerType LoadBalancerType { get; set; } = LoadBalancerType.Application;
    public ElasticBeanstalkSharedLoadBalancerProps? SharedLoadBalancerProps { get; set; }
    public ProtocolType ProtocolType { get; set; } = ProtocolType.Http;
    //The below are only required if the Load Balancer is handling HTTPS traffic
    public bool HttpsListenderEnabled { get; set; }
    public IEnumerable<string> SslCertificateArns { get; set; } = new List<string>();
}

public class ElasticBeanstalkSharedLoadBalancerProps 
{
    public string? SharedLoadBalancerArn { get; set; }
    public int SharedLoadBalancerListenderPort { get; set; }
}

public enum LoadBalancerType 
{
    Application,
    Network
}

public enum ProtocolType 
{
    Http,
    Https
}

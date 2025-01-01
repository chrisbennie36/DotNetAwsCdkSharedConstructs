using Amazon.CDK;

namespace AwsCdkSharedConstructs.ElasticBeanstalk;

public class ElasticBeanstalkProps : StackProps
{
    public string ExistingRoleName { get; set; } = "aws-elasticbeanstalk-ec2-role";
    public string InstanceProfileName { get; set; } = string.Empty;
    public string AssetPath { get; set; } = "../application.zip";
    public string ApplicationName { get; set; } = string.Empty;
    public string ApplicationId { get; set; } = string.Empty;
    public string RootVolumeType { get; set; } = "gp3";
    public string AutoScalingMaxSize { get; set; } = "1";
    public string AutoScalingMinSize { get; set; } = "1";
    public string EnvironmentName { get; set; } = "Projects"; //Must be > 4 chars
    public string SolutionStackName { get; set; } = "64bit Amazon Linux 2023 v3.2.1 running .NET 8";
}

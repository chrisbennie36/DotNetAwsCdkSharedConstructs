using System.Text;
using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ElasticBeanstalk;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.S3.Assets;
using Constructs;

namespace AwsCdkSharedConstructs.ElasticBeanstalk;

public class ElasticBeanstalk()
{
    public CfnApplication ElasticBeanstalkApplication { get; set; }

    public void Build(Construct scope, ElasticBeanstalkProps props)
    {
        IRole role = Role.FromRoleName(scope, $"{props.ApplicationId}-eb-app-role", props.ExistingRoleName);

        string instanceProfileId = $"{props.ApplicationId}-eb-instance-profile";

        var instanceProfile = new InstanceProfile(scope, instanceProfileId, new InstanceProfileProps
        {
            Role = role,
            InstanceProfileName = props.InstanceProfileName
        });

        _ = new CfnOutput(scope, $"{props.ApplicationId}-eb-iam-role", new CfnOutputProps 
        {
            Value = role.RoleArn
        });

        var archive = new Asset(scope, $"{props.ApplicationId}-app-zip-location", new AssetProps
        {
            Path = props.AssetPath
        });

        ElasticBeanstalkApplication = new Amazon.CDK.AWS.ElasticBeanstalk.CfnApplication(scope, $"{props.ApplicationId}-elb-app", new CfnApplicationProps 
        {
            ApplicationName = props.ApplicationName
        });

        CfnApplicationVersion applicationVersion = new Amazon.CDK.AWS.ElasticBeanstalk.CfnApplicationVersion(scope, $"{props.ApplicationId}-elb-app-version", new CfnApplicationVersionProps
        {
            ApplicationName = props.ApplicationName,
            SourceBundle = new Amazon.CDK.AWS.ElasticBeanstalk.CfnApplicationVersion.SourceBundleProperty
            {
                S3Bucket = archive.S3BucketName,
                S3Key = archive.S3ObjectKey
            }
        });

        CfnEnvironment elasticBeanstalkEnvironment = new CfnEnvironment(scope, $"{props.ApplicationId}-elb-environment", new CfnEnvironmentProps
        {
            ApplicationName = props.ApplicationName,
            OptionSettings = new CfnEnvironment.OptionSettingProperty[] 
            {
                new CfnEnvironment.OptionSettingProperty { Namespace = "aws:autoscaling:launchconfiguration", OptionName = "IamInstanceProfile", Value = instanceProfile.InstanceProfileArn },
                new CfnEnvironment.OptionSettingProperty { Namespace = "aws:autoscaling:launchconfiguration", OptionName = "RootVolumeType", Value = props.RootVolumeType },
                new CfnEnvironment.OptionSettingProperty { Namespace = "aws:autoscaling:asg", OptionName = "MaxSize", Value = props.AutoScalingMaxSize },
                new CfnEnvironment.OptionSettingProperty { Namespace = "aws:autoscaling:asg", OptionName = "MinSize", Value = props.AutoScalingMinSize },
            },
            EnvironmentName = props.EnvironmentName,
            SolutionStackName = props.SolutionStackName,
            VersionLabel = applicationVersion.Ref   //Critical apparently
        });

        if(props.UseElasticIp)
        {
            _ = new CfnEIP(scope, $"{props.ApplicationId}-elastic-ip", new CfnEIPProps 
            {
                InstanceId = instanceProfileId
            });
        }

        if(props.LoadBalancerProps != null)
        {
            AddLoadBalancer(elasticBeanstalkEnvironment, props.LoadBalancerProps);
        }

        elasticBeanstalkEnvironment.AddDependency(ElasticBeanstalkApplication);
        applicationVersion.AddDependency(ElasticBeanstalkApplication);
    }

    private void AddLoadBalancer(CfnEnvironment cfnEnvironment, ElasticBeanstalkLoadBalancerProps props)
    {
        List<CfnEnvironment.OptionSettingProperty> elasticBeanstalkLoadBalancerSettings = new List<CfnEnvironment.OptionSettingProperty>
        {
            new CfnEnvironment.OptionSettingProperty { Namespace = "aws:elasticbeanstalk:environment", OptionName = "LoadBalancerType", Value = props.LoadBalancerType.ToString().ToLower() },
            new CfnEnvironment.OptionSettingProperty { Namespace = "aws:elbv2:listener:default", OptionName = "Protocol", Value = props.ProtocolType.ToString().ToUpper() }
        }; 

        foreach(string certificateArn in props.SslCertificateArns)
        {
            elasticBeanstalkLoadBalancerSettings.Add(new CfnEnvironment.OptionSettingProperty { Namespace = "aws:elbv2:listender:default", OptionName = "SSLCertificateArns", Value = certificateArn });
        }

        if(props.SharedLoadBalancerProps != null)
        {
            AddSharedLoadBalancer(props.SharedLoadBalancerProps, elasticBeanstalkLoadBalancerSettings);
        }

        List<CfnEnvironment.OptionSettingProperty> elasticBeanstalkSettings = ((CfnEnvironment.OptionSettingProperty[])cfnEnvironment.OptionSettings).ToList();

        elasticBeanstalkSettings.AddRange(elasticBeanstalkLoadBalancerSettings);

        cfnEnvironment.OptionSettings = elasticBeanstalkSettings.ToArray();
    }

    private void AddSharedLoadBalancer(ElasticBeanstalkSharedLoadBalancerProps props, List<CfnEnvironment.OptionSettingProperty> loadBalancerSettings)
    {
        List<CfnEnvironment.OptionSettingProperty> sharedLoadBalancerSettings = new List<CfnEnvironment.OptionSettingProperty>
        {
            new CfnEnvironment.OptionSettingProperty { Namespace = "aws:elasticbeanstalk:environment", OptionName = "IsShared", Value = "true" },
            new CfnEnvironment.OptionSettingProperty { Namespace = "aws:elbv2:loadbalancer", OptionName = "SharedLoadBalancer", Value = props.SharedLoadBalancerArn}
        };

        loadBalancerSettings.AddRange(sharedLoadBalancerSettings);
    }
}

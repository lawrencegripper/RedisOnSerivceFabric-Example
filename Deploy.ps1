$ErrorActionPreference = "Stop"

##Update this command to deploy to remote cluster as needed. 
Connect-ServiceFabricCluster localhost:19000

Write-Host 'Copying application package...'
##When deploying to azure cluster remove the imagestoreconnectionstring parameter
Copy-ServiceFabricApplicationPackage -ApplicationPackagePath '.\RedisHost' -ImageStoreConnectionString 'file:C:\SfDevCluster\Data\ImageStoreShare' -ApplicationPackagePathInImageStore 'Store\redishost'

Write-Host 'Registering application type...'
Test-ServiceFabricApplicationPackage C:\Users\lawrencegripper\Source\Repos\RedisOnServiceFabric\RedisHost\

##For testing - clean up any existing deployments
$service = get-ServiceFabricApplication -ApplicationName 'fabric:/redishost'
if ($service)
{
    remove-servicefabricapplication $service.ApplicationName -force
}


$types = get-servicefabricapplicationtype RedisHostAppType
if ($types)
{
    foreach ($type in $types)
    {
        Unregister-ServiceFabricApplicationType $type.ApplicationTypeName -ApplicationTypeVersion  $type.ApplicationTypeVersion -force
    }
}


##Now lets register it again and deploy
Register-ServiceFabricApplicationType -ApplicationPathInImageStore 'Store\redishost'

New-ServiceFabricApplication -ApplicationName 'fabric:/redishost' -ApplicationTypeName 'RedisHostAppType' -ApplicationTypeVersion 1.0.0.3

#New-ServiceFabricService -ApplicationName 'fabric:/redishost' -ServiceName 'fabric:/redishost/redishostservice' -ServiceTypeName RedisHost -stateless -instancecount 1 -PartitionSchemeSingleton

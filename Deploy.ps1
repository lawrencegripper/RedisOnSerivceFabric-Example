$ErrorActionPreference = "Stop"

##Update this command to deploy to remote cluster as needed. 
Connect-ServiceFabricCluster localhost:19000

Write-Host 'Copying application package...'
##When deploying to azure cluster remove the imagestoreconnectionstring parameter
Copy-ServiceFabricApplicationPackage -ApplicationPackagePath '.\RedisHost' -ImageStoreConnectionString 'file:C:\SfDevCluster\Data\ImageStoreShare' -ApplicationPackagePathInImageStore 'Store\redishost'

Write-Host 'Registering application type...'
Test-ServiceFabricApplicationPackage C:\Users\lawrencegripper\Source\Repos\RedisOnServiceFabric\RedisHost\

$types = get-servicefabricapplicationtype RedisHostAppType
if ($types)
{
    foreach ($type in $types)
    {
        Unregister-ServiceFabricApplicationType $type.ApplicationTypeName
    }
}

Register-ServiceFabricApplicationType -ApplicationPathInImageStore 'Store\redishost'


$service = get-ServiceFabricApplication -ApplicationName 'fabric:/redishost'
if ($service)
{
    remove-servicefabricapplication $service.ApplicationName -force
}


New-ServiceFabricApplication -ApplicationName 'fabric:/redishost' -ApplicationTypeName 'RedisHostAppType' -ApplicationTypeVersion 1.0

New-ServiceFabricService -ApplicationName 'fabric:/redishost' -ServiceName 'fabric:/redishost/redishostservice' -ServiceTypeName RedisHost -stateless -instancecount 1 -PartitionSchemeSingleton

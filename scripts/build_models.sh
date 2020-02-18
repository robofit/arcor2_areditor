#!/bin/bash

if ! hash mcs 2>/dev/null; then
	echo "To build models dll, mono must be installed from repository (more info: https://www.mono-project.com/download/stable/#download-lin): "
	echo "	sudo apt install gnupg ca-certificates"
	echo "	sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF"
	echo '	echo "deb https://download.mono-project.com/repo/ubuntu stable-bionic main" | sudo tee /etc/apt/sources.list.d/mono-official-stable.list'
	echo "	sudo apt update"
    echo "	sudo apt install mono-devel mono-mcs"
    exit 1
fi


output="$2"

if [ "$1" = "" ]; then
    echo "Usage: build_models MODELS_FILE [OUTPUT_FOLDER]"
    exit 1
fi
if [ "$2" = "" ]; then
    output="output"
fi
if [ -d "./$output" ]; then
	echo "Directory $output already exist. Remove it first or specify other directory with optiaonal parameter."
	exit 1
fi



tmpfile=$(mktemp ./tmp.XXXXXX)
tmpconf=$(mktemp ./conf.XXXXXX)
tmpoutput=$(mktemp -d ./output.XXXXXX)


echo '{\n"optionalEmitDefaultValues": true,\n"packageName": "IO.Swagger"\n}' > "$tmpconf"

sed 's/      - properties: {}/      - properties:\n          data:\n            default: ""\n            type: string/g' "$1" | perl -0777 -pe's/format: uuid\n/\n/g' | perl -0777 -pe's/properties:\n        allowed_values:\n          type: array\n          uniqueItems: true/properties:\n        allowed_values:\n          type: array\n          uniqueItems: true\n          items:\n            {}/g' > "$tmpfile"


sudo docker run --rm -v ${PWD}:/local openapitools/openapi-generator-cli generate -i /local/"$tmpfile" -g csharp -o /local/"$tmpoutput" -c /local/"$tmpconf" || exit 1
rm "$tmpconf"
sudo chown -R `whoami` "$tmpoutput"

sed -i'' 's/DataMember(Name="data", EmitDefaultValue=true)/DataMember(Name="data", EmitDefaultValue=false)/' ./"$tmpoutput"/src/IO.Swagger/Model/SceneChangedEvent.cs
sed -i'' 's/DataMember(Name="data", EmitDefaultValue=true)/DataMember(Name="data", EmitDefaultValue=false)/' ./"$tmpoutput"/src/IO.Swagger/Model/ProjectChangedEvent.cs
sed -i'' 's/DataMember(Name="box", EmitDefaultValue=true)/DataMember(Name="box", EmitDefaultValue=false)/' ./"$tmpoutput"/src/IO.Swagger/Model/ObjectModel.cs
sed -i'' 's/DataMember(Name="cylinder", EmitDefaultValue=true)/DataMember(Name="cylinder", EmitDefaultValue=false)/' ./"$tmpoutput"/src/IO.Swagger/Model/ObjectModel.cs
sed -i'' 's/DataMember(Name="mesh", EmitDefaultValue=true)/DataMember(Name="mesh", EmitDefaultValue=false)/' ./"$tmpoutput"/src/IO.Swagger/Model/ObjectModel.cs
sed -i'' 's/DataMember(Name="sphere", EmitDefaultValue=true)/DataMember(Name="sphere", EmitDefaultValue=false)/' ./"$tmpoutput"/src/IO.Swagger/Model/ObjectModel.cs
sed -i'' 's/DataMember(Name="type", EmitDefaultValue=true)/DataMember(Name="type", EmitDefaultValue=false)/' ./"$tmpoutput"/src/IO.Swagger/Model/ObjectModel.cs
sed -i'' 's/DataMember(Name="object_model", EmitDefaultValue=true)/DataMember(Name="object_model", EmitDefaultValue=false)/' ./"$tmpoutput"/src/IO.Swagger/Model/ObjectTypeMeta.cs

#find ./"$tmpoutput"/src/IO.Swagger/Model -type f -exec sed -i.bak "s/regexUuid.Match(this.Uuid/regexUuid.Match(this.Uuid.ToString()/g" {} \;



echo "Models generated"


cd "$tmpoutput"
sh build.sh
cd ..
mkdir "$output" 
mv "$tmpoutput"/bin/* "$output"

rm -rf "$tmpoutput"

#rm "$tmpfile"

echo "Models in directory $output"


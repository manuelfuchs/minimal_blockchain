dotnet = dotnet
docker = docker
source_code_root = src
publish_directory = published
solution_file = ${source_code_root}/MinimalBlockchain.sln
api_proj = ${source_code_root}/MinimalBlockchain.Api/MinimalBlockchain.Api.csproj

.PHONY = run build clean publish cluster

run:
	@${dotnet} run --project ${api_proj}

build:
	@${dotnet} build ${solution_file}

clean:
	@echo "Cleaning ${publish_directory} directory"
	@rm -rf ${publish_directory}
	@mkdir ${publish_directory}
	@touch ${publish_directory}/.gitkeep
	@${dotnet} clean ${solution_file}

publish:
	@${dotnet} publish ${solution_file} -c Release --output ${publish_directory}

cluster: publish
	@${docker} compose up
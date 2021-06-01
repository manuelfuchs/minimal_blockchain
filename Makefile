dotnet = dotnet
source_code_root = src
publish_directory = out
solution_file = ${source_code_root}/MinimalBlockchain.sln
api_proj = ${source_code_root}/MinimalBlockchain.Api/MinimalBlockchain.Api.csproj

.PHONY = run build clean publish

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
	@${dotnet} publish ${solution_file} -c Release -r linux-x64 --self-contained true --output ${publish_directory}
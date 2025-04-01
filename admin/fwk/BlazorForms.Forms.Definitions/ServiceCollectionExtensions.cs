using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorForms.Forms;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddBlazorFormsDefinition([NotNull] this IServiceCollection services)
	{
		services.AddScoped(typeof(IFormDefinitionParser), typeof(FormDefinitionParser));
		
		return services;
	}
}

// WorkflowAutomation.API/Controllers/WorkflowsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkflowAutomation.Core.Entities;
using WorkflowAutomation.Core.Interfaces;
using WorkflowAutomation.Core.Models;
using WorkflowAutomation.Core.Enums;

namespace WorkflowAutomation.API.Controllers
{
    /// <summary>
    /// API controller for workflow management operations
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowsController : ControllerBase
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IWorkflowExecutor _workflowExecutor;

        public WorkflowsController(
            IWorkflowRepository workflowRepository,
            IWorkflowExecutor workflowExecutor)
        {
            _workflowRepository = workflowRepository;
            _workflowExecutor = workflowExecutor;
        }

        /// <summary>
        /// Get all workflows with optional environment filter
        /// </summary>
        /// <param name="environment">Optional environment filter (Testing, Launched, Production)</param>
        /// <returns>List of workflows</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<Workflow>), 200)]
        public async Task<ActionResult<List<Workflow>>> GetAll(
            [FromQuery] WorkflowEnvironment? environment = null)
        {
            IEnumerable<Workflow> workflows;

            if (environment.HasValue)
            {
                workflows = await _workflowRepository.GetByEnvironmentAsync(environment.Value);
            }
            else
            {
                workflows = await _workflowRepository.GetAllAsync();
            }

            return Ok(workflows);
        }

        /// <summary>
        /// Get a specific workflow by ID
        /// </summary>
        /// <param name="id">Workflow ID</param>
        /// <returns>Workflow details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Workflow), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Workflow>> GetById(Guid id)
        {
            var workflow = await _workflowRepository.GetByIdAsync(id);
            if (workflow == null)
                return NotFound(new { message = "Workflow not found" });

            return Ok(workflow);
        }

        /// <summary>
        /// Create a new workflow
        /// </summary>
        /// <param name="dto">Workflow creation data</param>
        /// <returns>Created workflow</returns>
        [HttpPost]
        [ProducesResponseType(typeof(Workflow), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<Workflow>> Create([FromBody] CreateWorkflowDto dto)
        {
            var workflow = new Workflow
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                Active = dto.Active,
                Environment = dto.Environment ?? WorkflowEnvironment.Testing,
                WorkflowData = System.Text.Json.JsonSerializer.Serialize(dto.Definition),
                Definition = dto.Definition,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = GetCurrentUserId(),
                Version = 1
            };

            await _workflowRepository.AddAsync(workflow);
            return CreatedAtAction(nameof(GetById), new { id = workflow.Id }, workflow);
        }

        /// <summary>
        /// Update an existing workflow
        /// </summary>
        /// <param name="id">Workflow ID</param>
        /// <param name="dto">Update data</param>
        /// <returns>Updated workflow</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Workflow), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Workflow>> Update(Guid id, [FromBody] UpdateWorkflowDto dto)
        {
            var workflow = await _workflowRepository.GetByIdAsync(id);
            if (workflow == null)
                return NotFound(new { message = "Workflow not found" });

            workflow.Name = dto.Name ?? workflow.Name;
            workflow.Description = dto.Description ?? workflow.Description;
            workflow.Active = dto.Active ?? workflow.Active;

            if (dto.Definition != null)
            {
                workflow.Definition = dto.Definition;
                workflow.WorkflowData = System.Text.Json.JsonSerializer.Serialize(dto.Definition);
                workflow.Version++;
            }

            workflow.UpdatedAt = DateTime.UtcNow;
            workflow.LastEditedAt = DateTime.UtcNow;
            workflow.LastEditedBy = GetCurrentUserId().ToString();

            await _workflowRepository.UpdateAsync(workflow);
            return Ok(workflow);
        }

        /// <summary>
        /// Delete a workflow
        /// </summary>
        /// <param name="id">Workflow ID</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Delete(Guid id)
        {
            var workflow = await _workflowRepository.GetByIdAsync(id);
            if (workflow == null)
                return NotFound(new { message = "Workflow not found" });

            await _workflowRepository.DeleteAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Activate a workflow
        /// </summary>
        /// <param name="id">Workflow ID</param>
        /// <returns>No content</returns>
        [HttpPost("{id}/activate")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Activate(Guid id)
        {
            var workflow = await _workflowRepository.GetByIdAsync(id);
            if (workflow == null)
                return NotFound(new { message = "Workflow not found" });

            workflow.Active = true;
            workflow.UpdatedAt = DateTime.UtcNow;
            await _workflowRepository.UpdateAsync(workflow);

            return NoContent();
        }

        /// <summary>
        /// Deactivate a workflow
        /// </summary>
        /// <param name="id">Workflow ID</param>
        /// <returns>No content</returns>
        [HttpPost("{id}/deactivate")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Deactivate(Guid id)
        {
            var workflow = await _workflowRepository.GetByIdAsync(id);
            if (workflow == null)
                return NotFound(new { message = "Workflow not found" });

            workflow.Active = false;
            workflow.UpdatedAt = DateTime.UtcNow;
            await _workflowRepository.UpdateAsync(workflow);

            return NoContent();
        }

        /// <summary>
        /// Execute a workflow manually
        /// </summary>
        /// <param name="id">Workflow ID</param>
        /// <param name="triggerData">Optional trigger data</param>
        /// <returns>Workflow execution details</returns>
        [HttpPost("{id}/execute")]
        [ProducesResponseType(typeof(WorkflowExecution), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<WorkflowExecution>> Execute(
            Guid id,
            [FromBody] Dictionary<string, object>? triggerData = null)
        {
            var workflow = await _workflowRepository.GetByIdAsync(id);
            if (workflow == null)
                return NotFound(new { message = "Workflow not found" });

            var execution = await _workflowExecutor.ExecuteAsync(
                workflow,
                triggerData ?? new Dictionary<string, object>());

            return Ok(execution);
        }

        /// <summary>
        /// Promote workflow to another environment
        /// </summary>
        /// <param name="id">Source workflow ID</param>
        /// <param name="dto">Promotion details</param>
        /// <returns>Promoted workflow</returns>
        [HttpPost("{id}/promote")]
        [ProducesResponseType(typeof(Workflow), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<Workflow>> PromoteToEnvironment(
            Guid id,
            [FromBody] PromoteWorkflowDto dto)
        {
            var sourceWorkflow = await _workflowRepository.GetByIdAsync(id);
            if (sourceWorkflow == null)
                return NotFound(new { message = "Workflow not found" });

            // Validate environment promotion path
            if (!IsValidPromotion(sourceWorkflow.Environment, dto.TargetEnvironment))
            {
                return BadRequest(new { message = "Invalid environment promotion path" });
            }

            // Create a new workflow in the target environment
            var promotedWorkflow = new Workflow
            {
                Id = Guid.NewGuid(),
                Name = sourceWorkflow.Name,
                Description = sourceWorkflow.Description,
                Active = false,
                Environment = dto.TargetEnvironment,
                WorkflowData = sourceWorkflow.WorkflowData,
                Definition = sourceWorkflow.Definition,
                ParentWorkflowId = sourceWorkflow.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = GetCurrentUserId(),
                Version = 1
            };

            await _workflowRepository.AddAsync(promotedWorkflow);
            return Ok(promotedWorkflow);
        }

        /// <summary>
        /// Import a workflow from JSON
        /// </summary>
        /// <param name="dto">Import data</param>
        /// <returns>Imported workflow</returns>
        [HttpPost("import")]
        [ProducesResponseType(typeof(Workflow), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<Workflow>> Import([FromBody] ImportWorkflowDto dto)
        {
            var workflow = new Workflow
            {
                Id = Guid.NewGuid(),
                Name = dto.Name ?? "Imported Workflow",
                Description = dto.Description,
                Active = false,
                Environment = WorkflowEnvironment.Testing,
                WorkflowData = System.Text.Json.JsonSerializer.Serialize(dto.Definition),
                Definition = dto.Definition,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = GetCurrentUserId(),
                Version = 1
            };

            await _workflowRepository.AddAsync(workflow);
            return CreatedAtAction(nameof(GetById), new { id = workflow.Id }, workflow);
        }

        /// <summary>
        /// Export a workflow to JSON
        /// </summary>
        /// <param name="id">Workflow ID</param>
        /// <returns>Workflow export data</returns>
        [HttpGet("{id}/export")]
        [ProducesResponseType(typeof(ExportWorkflowDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ExportWorkflowDto>> Export(Guid id)
        {
            var workflow = await _workflowRepository.GetByIdAsync(id);
            if (workflow == null)
                return NotFound(new { message = "Workflow not found" });

            var export = new ExportWorkflowDto
            {
                Name = workflow.Name,
                Description = workflow.Description,
                Definition = workflow.Definition,
                Version = "1.0",
                ExportedAt = DateTime.UtcNow
            };

            return Ok(export);
        }

        /// <summary>
        /// Get current user ID from JWT claims
        /// </summary>
        /// <returns>User ID</returns>
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value;
            if (userIdClaim != null && Guid.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }

            // Fallback to a default value if claim is not found
            return Guid.Empty;
        }

        /// <summary>
        /// Validate environment promotion path
        /// </summary>
        private bool IsValidPromotion(WorkflowEnvironment source, WorkflowEnvironment target)
        {
            // Testing -> Launched -> Production
            return (source == WorkflowEnvironment.Testing && target == WorkflowEnvironment.Launched) ||
                   (source == WorkflowEnvironment.Launched && target == WorkflowEnvironment.Production);
        }
    }

    // DTOs for Workflow Operations

    /// <summary>
    /// DTO for creating a new workflow
    /// </summary>
    public class CreateWorkflowDto
    {
        /// <summary>
        /// Workflow name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Workflow description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Whether the workflow is active
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Target environment (defaults to Testing)
        /// </summary>
        public WorkflowEnvironment? Environment { get; set; }

        /// <summary>
        /// Workflow definition (nodes and connections)
        /// </summary>
        public WorkflowDefinition Definition { get; set; } = new WorkflowDefinition();
    }

    /// <summary>
    /// DTO for updating a workflow
    /// </summary>
    public class UpdateWorkflowDto
    {
        /// <summary>
        /// Workflow name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Workflow description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Whether the workflow is active
        /// </summary>
        public bool? Active { get; set; }

        /// <summary>
        /// Updated workflow definition
        /// </summary>
        public WorkflowDefinition? Definition { get; set; }
    }

    /// <summary>
    /// DTO for promoting workflow to another environment
    /// </summary>
    public class PromoteWorkflowDto
    {
        /// <summary>
        /// Target environment for promotion
        /// </summary>
        public WorkflowEnvironment TargetEnvironment { get; set; }
    }

    /// <summary>
    /// DTO for importing a workflow
    /// </summary>
    public class ImportWorkflowDto
    {
        /// <summary>
        /// Workflow name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Workflow description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Workflow definition
        /// </summary>
        public WorkflowDefinition Definition { get; set; } = new WorkflowDefinition();
    }

    /// <summary>
    /// DTO for exporting a workflow
    /// </summary>
    public class ExportWorkflowDto
    {
        /// <summary>
        /// Workflow name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Workflow description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Workflow definition
        /// </summary>
        public WorkflowDefinition Definition { get; set; } = new WorkflowDefinition();

        /// <summary>
        /// Export format version
        /// </summary>
        public string Version { get; set; } = "1.0";

        /// <summary>
        /// Export timestamp
        /// </summary>
        public DateTime ExportedAt { get; set; }
    }
}

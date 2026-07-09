using Crm.Application.DOTs.Contact;
using Crm.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crm.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ContactsController : ControllerBase
    {
        private readonly IContactService _contactService;

        public ContactsController(IContactService contactService)
        {
            _contactService = contactService;
        }

        // ✅ GET: api/contacts
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var contacts = await _contactService.GetAllContactsAsync();
            return Ok(contacts);
        }

        // ✅ GET: api/contacts/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var contact = await _contactService.GetContactByIdAsync(id);
            if (contact == null)
                return NotFound($"Contact with ID {id} not found.");

            return Ok(contact);
        }

        // ✅ POST: api/contacts
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateContactDto dto)
        {
            var created = await _contactService.CreateContactAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // ✅ PUT: api/contacts/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateContactDto dto)
        {
            if (id != dto.Id)
                return BadRequest("Mismatched contact ID.");

            var updated = await _contactService.UpdateContactAsync(dto);
            return Ok(updated);
        }

        // ✅ DELETE: api/contacts/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _contactService.DeleteContactAsync(id);
            if (!success)
                return NotFound($"Contact with ID {id} not found.");

            return Ok(new { message = "Contact deleted successfully." });
        }

        // ✅ PUT: api/contacts/restore/{id}
        [HttpPut("restore/{id:guid}")]
        public async Task<IActionResult> Restore(Guid id)
        {
            var result = await _contactService.RestoreContactAsync(id);
            if (!result)
                return NotFound("Contact not found or already active.");

            return Ok(new { message = "Contact restored successfully." });
        }
    }
}
